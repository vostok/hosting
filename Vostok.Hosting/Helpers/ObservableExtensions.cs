using System;
using Vostok.Commons.Threading;
using Vostok.Configuration.Abstractions.Extensions.Observable;

namespace Vostok.Hosting.Helpers;

internal static class ObservableExtensions
{
    public static void SubscribeOnce<T>(this IObservable<T> observable, Action<T> action)
    {
        AtomicBoolean launched = false;
        IDisposable subscription = null;
        subscription = observable.Subscribe(t =>
        {
            if (!launched.TrySetTrue())
                return;

            action(t);
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once AccessToModifiedClosure
            subscription.Dispose();
            subscription = null;
        });
    }
}