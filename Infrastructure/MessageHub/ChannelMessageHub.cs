using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using Serilog;

namespace maxbl4.Infrastructure.MessageHub
{
    /// <summary>
    /// MessageHUb based on Channel<T> with dedicated dispatcher thread.
    /// All subscriptions are processed in series.
    /// </summary>
    public class ChannelMessageHub: IMessageHub
    {
        private volatile bool disposed = false;
        private readonly ILogger logger = Log.ForContext<ChannelMessageHub>();
        private readonly Channel<object> channel = Channel.CreateUnbounded<object>();
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        private List<Subscription> subscriptions = new List<Subscription>();
        private Stopwatch sw;

        public ChannelMessageHub()
        {
            sw = Stopwatch.StartNew();
            _ = ChannelReader();
        }

        async Task ChannelReader()
        {
            try
            {
                while (!disposed)
                {
                    var message = await channel.Reader.ReadAsync();
                    if (message == null) continue;

                    rwLock.EnterReadLock();
                    var currentSubs = subscriptions;
                    rwLock.ExitReadLock();
                    
                    foreach (var sub in currentSubs.Where(x => x.MessageType.IsInstanceOfType(message)))
                    {
                        if (sub.IsAsync)
                            await logger.Swallow(() => sub.InvokeAsync(message));
                        else
                            logger.Swallow(() => sub.Invoke(message));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal("ChannelMessageHub reader loop failed", ex);
            }
        }
        
        public void Publish<T>(T message)
        {
            channel.Writer.TryWrite(message);
        }

        public IDisposable Subscribe<T>(Action<T> action)
        {
            try
            {
                rwLock.EnterWriteLock();
                var sub = new Subscription<T>(this)
                {
                    Handler = action
                };
                var subs = subscriptions.ToList();
                subs.Add(sub);
                subscriptions = subs; 
                return sub;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public IDisposable SubscribeAsync<T>(Func<T, Task> action)
        {
            try
            {
                rwLock.EnterWriteLock();
                var sub = new Subscription<T>(this) { AsyncHandler = action };
                var subs = subscriptions.ToList();
                subs.Add(sub);
                subscriptions = subs;
                return sub;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            disposed = true;
        }

        void Unsubscribe(Subscription sub)
        {
            try
            {
                rwLock.EnterWriteLock();
                var subs = subscriptions.ToList();
                subs.Remove(sub);
                subscriptions = subs;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        abstract class Subscription: IDisposable
        {
            private readonly ChannelMessageHub owner;
            public Type MessageType { get; set; }

            public Subscription(ChannelMessageHub owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                owner.Unsubscribe(this);
            }

            public abstract void Invoke(object message);
            public abstract Task InvokeAsync(object message);
            public abstract bool IsAsync { get; }
        }

        class Subscription<T>: Subscription
        {
            public Subscription(ChannelMessageHub owner): base(owner)
            {
                MessageType = typeof(T);
            }

            public override bool IsAsync => AsyncHandler != null;
            
            public override void Invoke(object message)
            {
                Handler((T)message);
            }
            
            public override Task InvokeAsync(object message)
            {
                return AsyncHandler((T)message);
            }
            
            public Action<T> Handler { get; set; }
            public Func<T, Task> AsyncHandler { get; set; }
        }
        
        public class TestHubMessage
        {
            public static int StartIndex = 1;
            public string Content { get; set; }
            public int Index { get; set; } = StartIndex++;

            public override string ToString()
            {
                return $"{Index} {Content}";
            }
        }
    }
}