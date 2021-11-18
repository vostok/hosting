using System;
using System.Collections.Generic;
using Vostok.Commons.Helpers.Disposable;

namespace Vostok.Hosting.Helpers
{
    internal class PropagateObservable<T> : IObservable<T>, IObserver<T>
    {
        private readonly object guard = new();
        private volatile IDisposable observableSubscription;
        private volatile IObservable<T> baseObservable;
        private readonly List<IObserver<T>> observers = new();

        public void SetBaseObservable(IObservable<T> observable)
        {
            lock (guard)
            {
                observableSubscription?.Dispose();
                baseObservable = observable;
                observableSubscription = baseObservable.Subscribe(this);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (guard)
            {
                observers.Add(observer);
            }

            return new ActionDisposable(() =>
            {
                lock (guard)
                {
                    observers.Remove(observer);
                }
            });
        }

        public void OnCompleted()
        {
            lock (guard)
            {
                foreach (var observer in observers)
                    observer.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            lock (guard)
            {
                foreach (var observer in observers)
                    observer.OnError(error);
            }
        }

        public void OnNext(T value)
        {
            lock (guard)
            {
                foreach (var observer in observers)
                    observer.OnNext(value);
            }
        }
    }
}