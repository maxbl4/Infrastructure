using System.IO;
using Shouldly;
using Xunit;

namespace maxbl4.Infrastructure.Tests
{
    public class RollingFileTests
    {
        [Fact]
        public void Should_return_original_for_non_existing()
        {
            RollingFile.GetNext("aaa").ShouldBe("aaa");
            RollingFile.GetCurrent("aaa").ShouldBe("aaa");
            var full = Path.GetFullPath("aaa");
            full.Length.ShouldBeGreaterThan(3);
            RollingFile.GetNext(full).ShouldBe(full);
        }
        
        [Fact]
        public void Should_return_next()
        {
            File.WriteAllText("bb.txt", "text");
            RollingFile.GetNext("bb.txt").ShouldBe("bb_001.txt");
            RollingFile.GetCurrent("bb.txt").ShouldBe("bb.txt");
        }
        
        [Fact]
        public void Should_return_next_next()
        {
            File.WriteAllText("cc.txt", "text");
            File.WriteAllText("cc_001.txt", "text");
            RollingFile.GetNext("cc.txt").ShouldBe("cc_002.txt");
            RollingFile.GetCurrent("cc.txt").ShouldBe("cc_001.txt");
        }
        
        [Fact]
        public void Should_return_next_power()
        {
            File.WriteAllText("dd.txt", "text");
            for (int i = 1; i < 10; i++)
            {
                File.WriteAllText($"dd_{i}.txt", "text");
            }
            RollingFile.GetNext("dd.txt", 1).ShouldBe("dd_01.txt");
            RollingFile.GetCurrent("dd.txt", 1).ShouldBe("dd_9.txt");
        }
    }
}