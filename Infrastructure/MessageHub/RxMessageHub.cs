using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using Serilog;

namespace maxbl4.Infrastructure.MessageHub
{
    public class RxMessageHub: IMessageHub
    {
        private readonly ILogger logger = Log.ForContext<RxMessageHub>();
        private readonly Subject<object> messages = new Subject<object>();
        
        public void Publish<T>(T message)
        {
            messages.OnNext(message);
        }

        public IDisposable Subscribe<T>(Action<T> action)
        {
            return messages.OfType<T>().Subscribe(x =>
            {
                logger.Swallow(() => action(x));
            });
        }

        public IDisposable SubscribeAsync<T>(Func<T, Task> action)
        {
            return messages.OfType<T>()
                .Select(x => Observable.FromAsync(() => logger.Swallow(async () => await action(x))))
                .Concat()
                .Subscribe();
        }

        public void Dispose()
        {
            messages.DisposeSafe();
        }
    }
}