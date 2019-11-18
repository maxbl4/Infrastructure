using System;
using maxbl4.RaceLogic.Tests;
using maxbl4.RfidDotNet.Infrastructure;
using Shouldly;
using Xunit;

namespace maxbl4.Infrastructure.Tests
{
    public class TimingTests
    {
        [Fact]
        public void Should_throw_with_caller_name()
        {
            var details = 101;
            var logger = MemoryLogger.Serilog<TimingTests>();
            var ex = Assert.Throws<TimeoutException>(() =>
                new Timing()
                    .Logger(logger.instance, "ctx", () => $"{details++}")
                    .Timeout(10)
                    .Expect(() => false));
            ex.Message.ShouldMatch(@"\[Should_throw_with_caller_name\] Wait failed ctx after 00:00:00.\d+ details 101");
            logger.messages[0].ShouldBe("[maxbl4.Infrastructure.Tests.TimingTests] Should_throw_with_caller_name => Begin wait ctx");
            logger.messages[1].ShouldMatch(@"\[maxbl4.Infrastructure.Tests.TimingTests\] Should_throw_with_caller_name => Wait failed ctx after 00:00:00.\d+ details 101");
        }
        
        [Fact]
        public void Should_write_logs_on_success()
        {
            var details = 101;
            var logger = MemoryLogger.Serilog<TimingTests>();
            new Timing()
                    .Logger(logger.instance, "ctx", () => $"{details++}")
                    .Timeout(10)
                    .Expect(() => true);
            
            logger.messages[0].ShouldBe("[maxbl4.Infrastructure.Tests.TimingTests] Should_write_logs_on_success => Begin wait ctx");
            logger.messages[1].ShouldMatch(@"\[maxbl4.Infrastructure.Tests.TimingTests\] Should_write_logs_on_success => Wait success ctx after 00:00:00.\d+");
        }

        [Fact]
        public void Should_work_without_logger()
        {
            new Timing().Timeout(10).Wait(() => true).ShouldBeTrue();
        }
    }
}