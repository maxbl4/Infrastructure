using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using Serilog;

namespace maxbl4.Infrastructure.MessageHub
{
    public class ChannelMessageHub: IMessageHub
    {
        private volatile bool disposed = false;
        private readonly ILogger logger = Log.ForContext<ChannelMessageHub>();
        private readonly Channel<object> channel = Channel.CreateUnbounded<object>();
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        private readonly List<Subscription> subscriptions = new List<Subscription>();
        private Stopwatch sw;

        public ChannelMessageHub()
        {
            sw = Stopwatch.StartNew();
            _ = ChannelReader();
        }

        async Task ChannelReader()
        {
            Console.WriteLine($"{sw.ElapsedMilliseconds} ChannelReader Start");
            try
            {
                while (!disposed)
                {
                    Console.WriteLine($"{sw.ElapsedMilliseconds} {Thread.CurrentThread.ManagedThreadId} ChannelReader Start loop");
                    var message = await channel.Reader.ReadAsync();
                    Console.WriteLine($"{sw.ElapsedMilliseconds} {Thread.CurrentThread.ManagedThreadId} ChannelReader Read Message {(message as TestHubMessage)?.Index}");
                    if (message == null) continue;
                    List<Subscription> currentSubs;
                    try
                    {
                        rwLock.EnterReadLock();
                        currentSubs = subscriptions.ToList();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{sw.ElapsedMilliseconds} {Thread.CurrentThread.ManagedThreadId} ChannelReader Handler loop error {ex}");
                        continue;
                    }
                    finally
                    {
                        rwLock.ExitReadLock();
                    }
                    foreach (var sub in currentSubs.Where(x => x.MessageType.IsInstanceOfType(message)))
                    {
                        Console.WriteLine($"{sw.ElapsedMilliseconds} {Thread.CurrentThread.ManagedThreadId} ChannelReader Invoke handler {(message as TestHubMessage)?.Index}");
                        if (sub.IsAsync)
                            await logger.Swallow(() => sub.InvokeAsync(message));
                        else
                            logger.Swallow(() => _ = sub.InvokeAsync(message));
                        Console.WriteLine($"{sw.ElapsedMilliseconds} {Thread.CurrentThread.ManagedThreadId} ChannelReader handler completed {(message as TestHubMessage)?.Index}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{sw.ElapsedMilliseconds} ChannelReader Error {ex}");
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
                subscriptions.Add(sub);
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
                subscriptions.Add(sub);
                return sub;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            Console.WriteLine($"{sw.ElapsedMilliseconds} Hub disposing");
            disposed = true;
        }

        void Unsubscribe(Subscription sub)
        {
            try
            {
                Console.WriteLine($"{sw.ElapsedMilliseconds} Sub disposing");
                rwLock.EnterWriteLock();
                subscriptions.Remove(sub);
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
            
            public override Task InvokeAsync(object message)
            {
                if (Handler != null)
                {
                    Handler((T)message);
                    return Task.CompletedTask;
                }
                return AsyncHandler((T)message);
            }
            
            public Action<T> Handler { get; set; }
            public Func<T, Task> AsyncHandler { get; set; }
        }
    }
}