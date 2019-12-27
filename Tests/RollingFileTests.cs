using System.IO;
using FluentAssertions;
using Xunit;

namespace maxbl4.Infrastructure.Tests
{
    public class RollingFileTests
    {
        [Fact]
        public void Should_return_original_for_non_existing()
        {
            File.Exists("aaa").Should().BeFalse();
            var aaa = new RollingFileInfo("aaa");
            aaa.NextFile.Should().Be("aaa");
            aaa.CurrentFile.Should().Be("aaa");
            aaa.BaseExists.Should().BeFalse();
            aaa.Exist.Should().BeFalse();
            aaa.Index.Should().Be(0);
            var full = Path.GetFullPath("aaa");
            full.Length.Should().BeGreaterThan(3);
            new RollingFileInfo(full).NextFile.Should().Be(full);
        }
        
        [Fact]
        public void Should_return_next()
        {
            File.WriteAllText("bb.txt", "text");
            new RollingFileInfo("bb.txt").NextFile.Should().Be("bb_001.txt");
            new RollingFileInfo("bb.txt").CurrentFile.Should().Be("bb.txt");
        }
        
        [Fact]
        public void Should_work_without_extension()
        {
            File.WriteAllText("bbb", "text");
            new RollingFileInfo("bbb").NextFile.Should().Be("bbb_001");
        }
        
        [Fact]
        public void Should_return_next_next()
        {
            File.WriteAllText("cc.txt", "text");
            File.WriteAllText("cc_001.txt", "text");
            var cc = new RollingFileInfo("cc.txt");
            cc.Index.Should().Be(1);
            cc.NextFile.Should().Be("cc_002.txt");
            cc.CurrentFile.Should().Be("cc_001.txt");
            cc.AllCurrentFiles.Should().Equal(new [] {"cc.txt", "cc_001.txt"});
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
            dd.Index.Should().Be(9);
            dd.NextFile.Should().Be("dd_10.txt");
            dd.CurrentFile.Should().Be("dd_9.txt");
            dd.Exist.Should().BeTrue();
            dd.BaseExists.Should().BeTrue();
            dd.Delete().Should().Be(10);
            dd.Exist.Should().BeFalse();
            dd.BaseExists.Should().BeFalse();
            dd.CurrentFile.Should().Be("dd.txt");
            dd.NextFile.Should().Be("dd.txt");
        }
        
        [Fact]
        public void Should_find_rolled_files_when_base_not_exists()
        {
            File.WriteAllText("eee_005.txt", "text");
            var f = new RollingFileInfo("eee.txt");
            f.BaseExists.Should().BeFalse();
            f.Exist.Should().BeTrue();
            f.CurrentFile.Should().Be("eee_005.txt");
            f.NextFile.Should().Be("eee_006.txt");
            f.Index.Should().Be(5);
        }
        
        [Fact]
        public void Should_preserve_relative_paths()
        {
            Directory.CreateDirectory("fff");
            File.WriteAllText(@".\fff\..\fff.txt", "text");
            File.Exists("fff.txt").Should().BeTrue();
            var f = new RollingFileInfo(@".\fff\..\fff.txt");
            f.BaseExists.Should().BeTrue();
            f.Exist.Should().BeTrue();
            f.CurrentFile.Should().Be(@".\fff\..\fff.txt");
            f.NextFile.Should().Be(@".\fff\..\fff_001.txt");
        }
        
        [Fact]
        public void Should_not_throw_on_delete_not_existing()
        {
            File.Exists("ggg").Should().BeFalse();
            var aaa = new RollingFileInfo("ggg");
            aaa.Exist.Should().BeFalse();
            aaa.Delete().Should().Be(1);
        }
    }
}