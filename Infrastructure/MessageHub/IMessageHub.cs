using System;
using System.Threading.Tasks;

namespace maxbl4.Infrastructure.MessageHub
{
    public interface IMessageHub : IDisposable
    {
        void Publish<T>(T message);
        IDisposable Subscribe<T>(Action<T> action);
        IDisposable SubscribeAsync<T>(Func<T, Task> action);
    }
}