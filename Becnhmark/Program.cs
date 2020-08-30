using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.MessageHub;
using Serilog;

namespace Becnhmark
{
    class Program
    {
        static int counter = 0;
        static Dictionary<string, long> results = new Dictionary<string, long>();
        static Stopwatch sw = new Stopwatch();
        static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        
        static void Main(string[] args)
        {
            var hubs = new IMessageHub[]
            {
                new ChannelMessageHub(),
                new RxMessageHub(),
                new EasyMessageHub(),
            };
            for (var i = 0; i < 2; i++)
            foreach (var hub in hubs)
            {
                const int iterations = 1000000;
                var calls = 0;
                using var sub = hub.Subscribe<TestHubMessage>(x => Interlocked.Increment(ref counter));
                using var sub2 = hub.SubscribeAsync<TestHubMessage>(async x =>
                {
                    try
                    {
                        await _semaphoreSlim.WaitAsync();
                        await Task.Yield();
                        Interlocked.Increment(ref counter);
                        var v = Interlocked.Increment(ref calls);
                        if (calls % 10000 == 0)
                            Console.Write(".");
                        if (v == iterations)
                        {
                            sw.Stop();
                            Console.WriteLine();
                        }
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                });
                
                RunTest(hub, 9000);
                Console.WriteLine($"Benchmark {hub.GetType().Name}");
                RunTest(hub, iterations);
                sub2.DisposeSafe();
            }

            var baseValue = 0L;
            foreach (var res in results.OrderBy(x => x.Value))
            {
                if (baseValue == 0) baseValue = res.Value > 0 ? res.Value : 1;
                Console.WriteLine($"{res.Key} {res.Value} {100*res.Value/(baseValue)}%");
            }
        }

        static void RunTest(IMessageHub hub, int count)
        {
            sw.Restart();
            counter = 0;
            for (var i = 0; i < count; i++)
            {
                hub.Publish(new TestHubMessage {Index = i});
            }
            new Timing()
                .Timeout(30000)
                .Polling(TimeSpan.FromMilliseconds(1)).Expect(() => counter == count * 2);
            results[hub.GetType().Name] = sw.ElapsedMilliseconds;
        }
    }
}