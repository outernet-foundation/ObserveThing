using System;

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

        private bool _disposed;

        public Observer(Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
        {
            this.onNext = onNext;
            this.onError = onError;
            this.onDispose = onDispose;
        }

        public void OnNext(T args)
        {
            if (_disposed)
                return;

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
        {
            if (_disposed)
                return;

            if (onError != null)
            {
                onError(exception);
                return;
            }

            throw new Exception($"Observed an unhandled exception: {exception.Message}\n{exception.StackTrace}");
        }

        public void OnDispose()
        {
            if (_disposed)
                return;

            Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            onDispose?.Invoke();
        }
    }
}