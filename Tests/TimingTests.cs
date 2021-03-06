using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

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
                    .Logger(logger.instance)
                    .Context("ctx")
                    .FailureDetails(() => $"{details++}")
                    .Timeout(10)
                    .Expect(() => false));
            ex.Message.Should().MatchRegex(@"\[Should_throw_with_caller_name\] Wait failed ctx after 00:00:00.\d+ details 101");
            logger.messages[0].Should().Be("[maxbl4.Infrastructure.Tests.TimingTests] Should_throw_with_caller_name => Begin wait ctx");
            logger.messages[1].Should().MatchRegex(@"\[maxbl4.Infrastructure.Tests.TimingTests\] Should_throw_with_caller_name => Wait failed ctx after 00:00:00.\d+ details 101");
        }
        
        [Fact]
        public void Should_write_logs_on_success()
        {
            var details = 101;
            var logger = MemoryLogger.Serilog<TimingTests>();
            new Timing()
                    .Logger(logger.instance)
                    .Context("ctx")
                    .FailureDetails(() => $"{details++}")
                    .Timeout(10000)
                    .Expect(() => true);
            
            logger.messages[0].Should().Be("[maxbl4.Infrastructure.Tests.TimingTests] Should_write_logs_on_success => Begin wait ctx");
            logger.messages[1].Should().MatchRegex(@"\[maxbl4.Infrastructure.Tests.TimingTests\] Should_write_logs_on_success => Wait success ctx after 00:00:00.\d+");
        }

        [Fact]
        public void Should_work_without_logger()
        {
            new Timing().Timeout(10).Wait(() => true).Should().BeTrue();
        }
        
        [Fact]
        public async Task Should_wait_for_async()
        {
            var complete = false;
            await new Timing().Timeout(50).ExpectAsync(async () =>
            {
                await Task.Delay(10);
                complete = true;
                return true;
            });
            complete.Should().BeTrue();
        }
    }
}