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
            File.Exists("aaa").ShouldBeFalse();
            var aaa = new RollingFileInfo("aaa");
            aaa.NextFile.ShouldBe("aaa");
            aaa.CurrentFile.ShouldBe("aaa");
            aaa.BaseExists.ShouldBeFalse();
            aaa.Exist.ShouldBeFalse();
            var full = Path.GetFullPath("aaa");
            full.Length.ShouldBeGreaterThan(3);
            new RollingFileInfo(full).NextFile.ShouldBe(full);
        }
        
        [Fact]
        public void Should_return_next()
        {
            File.WriteAllText("bb.txt", "text");
            new RollingFileInfo("bb.txt").NextFile.ShouldBe("bb_001.txt");
            new RollingFileInfo("bb.txt").CurrentFile.ShouldBe("bb.txt");
        }
        
        [Fact]
        public void Should_work_without_extension()
        {
            File.WriteAllText("bbb", "text");
            new RollingFileInfo("bbb").NextFile.ShouldBe("bbb_001");
        }
        
        [Fact]
        public void Should_return_next_next()
        {
            File.WriteAllText("cc.txt", "text");
            File.WriteAllText("cc_001.txt", "text");
            new RollingFileInfo("cc.txt").NextFile.ShouldBe("cc_002.txt");
            new RollingFileInfo("cc.txt").CurrentFile.ShouldBe("cc_001.txt");
        }
        
        [Fact]
        public void Should_return_next_power()
        {
            File.WriteAllText("dd.txt", "text");
            for (var i = 1; i < 10; i++)
            {
                File.WriteAllText($"dd_{i}.txt", "text");
            }
            var dd = new RollingFileInfo("dd.txt", 1);
            dd.NextFile.ShouldBe("dd_10.txt");
            dd.CurrentFile.ShouldBe("dd_9.txt");
            dd.Exist.ShouldBeTrue();
            dd.BaseExists.ShouldBeTrue();
            dd.Delete();
            dd.Exist.ShouldBeFalse();
            dd.BaseExists.ShouldBeFalse();
            dd.CurrentFile.ShouldBe("dd.txt");
            dd.NextFile.ShouldBe("dd.txt");
        }
        
        [Fact]
        public void Should_find_rolled_files_when_base_not_exists()
        {
            File.WriteAllText("eee_005.txt", "text");
            var f = new RollingFileInfo("eee.txt");
            f.BaseExists.ShouldBeFalse();
            f.Exist.ShouldBeTrue();
            f.CurrentFile.ShouldBe("eee_005.txt");
            f.NextFile.ShouldBe("eee_006.txt");
        }
        
        [Fact]
        public void Should_preserve_relative_paths()
        {
            Directory.CreateDirectory("fff");
            File.WriteAllText(@".\fff\..\fff.txt", "text");
            File.Exists("fff.txt").ShouldBeTrue();
            var f = new RollingFileInfo(@".\fff\..\fff.txt");
            f.BaseExists.ShouldBeTrue();
            f.Exist.ShouldBeTrue();
            f.CurrentFile.ShouldBe(@".\fff\..\fff.txt");
            f.NextFile.ShouldBe(@".\fff\..\fff_001.txt");
        }
    }
}