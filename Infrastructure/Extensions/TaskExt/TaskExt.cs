using System.Threading.Tasks;

namespace maxbl4.Infrastructure.Extensions.TaskExt
{
    public static class TaskExt
    {
        private static readonly TaskFactory _taskFactory = new
            TaskFactory(default,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        public static void RunSync(this Task task)
            => _taskFactory.StartNew(() => task).Unwrap().GetAwaiter().GetResult();


        public static TResult RunSync<TResult>(this Task<TResult> task)
            => _taskFactory.StartNew(() => task).Unwrap().GetAwaiter().GetResult();

        public static TResult RunSync<TResult>(this ValueTask<TResult> task)
            => _taskFactory.StartNew(task.AsTask).Unwrap().GetAwaiter().GetResult();
    }
}