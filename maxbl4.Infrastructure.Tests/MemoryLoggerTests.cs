using maxbl4.RaceLogic.Tests;
using Shouldly;
using Xunit;

namespace maxbl4.Infrastructure.Tests
{
    public class MemoryLoggerTests
    {
        [Fact]
        public void Should_log_serilog_without_context()
        {
            var logger = MemoryLogger.Serilog();
            logger.instance.Debug("some");
            logger.messages.Count.ShouldBe(1);
            logger.messages[0].ShouldBe("some");
        }
        
        [Fact]
        public void Should_log_serilog_with_context()
        {
            var logger = MemoryLogger.Serilog<MemoryLoggerTests>();
            logger.instance.Debug("some");
            logger.messages.Count.ShouldBe(1);
            logger.messages[0].ShouldBe($"[{GetType().FullName}] some");
        }
    }
}