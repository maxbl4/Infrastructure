﻿using System;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.RaceLogic.Tests;
using Shouldly;
using Xunit;

namespace maxbl4.Infrastructure.Tests
{
    public class LoggerExtTests
    {
        [Fact]
        public void Should_return_completed_task_on_error()
        {
            var logger = MemoryLogger.Serilog();
            var t = logger.instance.Swallow(async () =>
            {
                await Task.Delay(1);
                throw new Exception();
            });
            t.Wait(5000).ShouldBeTrue();
            t.IsCompletedSuccessfully.ShouldBeTrue();
            logger.messages.Count.ShouldBe(1);
        }
        
        [Fact]
        public void Should_return_completed_task_on_success()
        {
            var logger = MemoryLogger.Serilog();
            var t = logger.instance.Swallow(async () =>
            {
                await Task.Delay(1);
            });
            t.Wait(5000).ShouldBeTrue();
            t.IsCompletedSuccessfully.ShouldBeTrue();
            logger.messages.Count.ShouldBe(0);
        }
    }
}