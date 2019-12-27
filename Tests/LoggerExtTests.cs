﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure.Extensions.LoggerExt;
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
            t.Wait(5000).Should().BeTrue();
            t.IsCompletedSuccessfully.Should().BeTrue();
            logger.messages.Count.Should().Be(1);
        }
        
        [Fact]
        public void Should_return_completed_task_on_success()
        {
            var logger = MemoryLogger.Serilog();
            var t = logger.instance.Swallow(async () =>
            {
                await Task.Delay(1);
            });
            t.Wait(5000).Should().BeTrue();
            t.IsCompletedSuccessfully.Should().BeTrue();
            logger.messages.Count.Should().Be(0);
        }
        
        [Fact]
        public void Should_call_on_error_and_log()
        {
            var logger = MemoryLogger.Serilog();
            logger.instance.Swallow(() =>
            {
                logger.instance.Information("aaaaaa");
                throw new ArgumentException();
            }, e =>
            {
                logger.instance.Information("bbbbbb");
                throw new ArgumentOutOfRangeException();
            });
            logger.messages.Count.Should().Be(4);
            logger.messages.Should().Contain(x => x.Contains("aaaaa"));
            logger.messages.Should().Contain(x => x.Contains("bbbbb"));
        }
    }
}