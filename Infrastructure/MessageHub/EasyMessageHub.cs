using System;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.MessageHubExt;

namespace maxbl4.Infrastructure.MessageHub
{
    public class EasyMessageHub: IMessageHub
    {
        private readonly Easy.MessageHub.IMessageHub _internalHub = new Easy.MessageHub.MessageHub();
        
        public void Dispose()
        {
            _internalHub.DisposeSafe();
        }

        public void Publish<T>(T message)
        {
            _internalHub.Publish(message);
        }

        public IDisposable Subscribe<T>(Action<T> action)
        {
            return _internalHub.SubscribeDisposable(action);
        }

        public IDisposable SubscribeAsync<T>(Func<T, Task> action)
        {
            return _internalHub.SubscribeDisposable<T>(async x => await action(x));
        }
    }
}