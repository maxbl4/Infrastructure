using System;
using System.Globalization;
using FluentAssertions;
using maxbl4.Infrastructure.Extensions.DateTimeExt;
using Xunit;

namespace maxbl4.Infrastructure.Tests
{
    public class DateTimeExtTests
    {
        [Fact]
        public void Should_parse_as_utc()
        {
            var parsed = DateTimeExt.ParseAsUtc("2019-11-22 19:07:11", "yyyy-MM-dd HH:mm:ss");
            parsed.Hour.Should().Be(19);
            parsed.Kind.Should().Be(DateTimeKind.Utc);
        }
        
        [Fact]
        public void Should_try_parse_as_utc()
        {
            DateTimeExt.TryParseAsUtc("2019-11-22 19:07:11", "yyyy-MM-dd HH:mm:ss", DateTimeStyles.AssumeLocal, out var parsed)
                .Should().BeTrue();
            parsed.Hour.Should().Be(19);
            parsed.Kind.Should().Be(DateTimeKind.Utc);
        }
    }
}