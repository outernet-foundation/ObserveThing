using System;
using System.Diagnostics;

namespace ObserveThing
{
    public interface IObserver<in T>
    {
        void OnNext(T args);
        void OnError(Exception exception);
        void OnDispose();
    }

    public sealed class Observer<T> : IObserver<T> where T : IObservableEventArgs
    {
        public Action<T> onNext { get; }
        public Action<Exception> onError { get; }
        public Action onDispose { get; }

        public Observer(Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
        {
            this.onNext = onNext;
            this.onError = onError ?? ThrowError;
            this.onDispose = onDispose;
        }

        private void ThrowError(Exception error)
        {
            throw new Exception($"Encountered exception while notifying observer. Exception: {error.Message}\n{error.StackTrace}");
        }

        public void OnNext(T args)
        {
            try
            {
                onNext?.Invoke(args);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnError(Exception exception)
            => onError?.Invoke(exception);

        public void OnDispose()
            => onDispose?.Invoke();

        public void Dispose()
            => OnDispose();
    }
}