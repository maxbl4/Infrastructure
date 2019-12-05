using maxbl4.Infrastructure.Extensions.TestOutputHelperExt;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Infrastructure.Tests
{
    public class TestOutputHelperTests
    {
        private readonly ITestOutputHelper outputHelper;

        public TestOutputHelperTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
            outputHelper.GetTestName().ShouldBe("maxbl4.Infrastructure.Tests.TestOutputHelperTests.Should_get_executing_test_name");
        }

        [Fact]
        public void Should_get_executing_test_name()
        {
            outputHelper.GetTestName().ShouldBe("maxbl4.Infrastructure.Tests.TestOutputHelperTests.Should_get_executing_test_name");
        }
    }
}