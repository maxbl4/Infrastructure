using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure.Extensions.TaskExt;
using Xunit;

namespace maxbl4.Infrastructure.Tests
{
    public class TaskExtTests
    {
        [Fact]
        public void Run_task_sync()
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(10);
                return 1;
            });
            task.RunSync().Should().Be(1);
        }
    }
}