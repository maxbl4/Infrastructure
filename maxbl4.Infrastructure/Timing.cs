﻿using System;
using System.Diagnostics;
 using System.Runtime.CompilerServices;
 using System.Threading.Tasks;
 using Serilog;

 namespace maxbl4.RfidDotNet.Infrastructure
{
    public class Timing
    {
        private TimeSpan timeout = TimeSpan.FromSeconds(10);
        private TimeSpan pollingInterval = TimeSpan.FromMilliseconds(100);
        private ILogger logger;
        private string context;
        private Lazy<string> failureDetails;
        private Stopwatch stopwatch;
        
        public Timing Timeout(TimeSpan timeout)
        {
            this.timeout = timeout;
            return this;
        }
        
        public Timing Timeout(int timeoutMs)
        {
            this.timeout = TimeSpan.FromMilliseconds(timeoutMs);
            return this;
        }
        
        public Timing Polling(TimeSpan pollingInterval)
        {
            this.pollingInterval = pollingInterval;
            return this;
        }
        
        public Timing Logger(ILogger logger)
        {
            this.logger = logger;
            return this;
        }
        
        public Timing Context(string context)
        {
            this.context = context;
            return this;
        }
        
        public Timing FailureDetails(Func<string> failureDetails)
        {
            this.failureDetails = new Lazy<string>(failureDetails);
            return this;
        }

        public async Task<bool> WaitAsync(Func<bool> condition, [CallerMemberName]string caller = default)
        {
            logger?.Debug($"{caller} => Begin wait {context}");
            stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < timeout)
            {
                if (condition())
                {
                    logger?.Debug($"{caller} => Wait success {context} after {stopwatch.Elapsed}", 
                        caller, context, stopwatch.Elapsed);
                    return true;
                }
                await Task.Delay(pollingInterval);
            }
            stopwatch.Stop();
            logger?.Debug($"{caller} => Wait failed {context} after {stopwatch.Elapsed} details {@failureDetails?.Value}");
            return false;
        }
        
        public bool Wait(Func<bool> condition, [CallerMemberName]string caller = default)
        {
            return WaitAsync(condition, caller).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Expect(Func<bool> condition, [CallerMemberName]string caller = default)
        {
            ExpectAsync(condition, caller).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
        public async Task ExpectAsync(Func<bool> condition, [CallerMemberName]string caller = default)
        {
            if (!await WaitAsync(condition, caller))
                throw new TimeoutException($"[{caller}] Wait failed {context} after {stopwatch.Elapsed} details {failureDetails?.Value}");
        }
    }
}