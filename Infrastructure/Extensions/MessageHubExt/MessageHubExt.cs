using System;
using System.Reactive.Disposables;

namespace maxbl4.Infrastructure.Extensions.MessageHubExt
{
    public static class MessageHubExt
    {
        public static IDisposable SubscribeDisposable<T>(this Easy.MessageHub.IMessageHub messageHub, Action<T> action)
        {
            var subId = messageHub.Subscribe(action);
            return Disposable.Create(() => messageHub.Unsubscribe(subId));
        }
    }
}