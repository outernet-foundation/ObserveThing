using System;

namespace ObserveThing
{
    public interface IObserver<in T>
    {
        void OnNext(T args);
        void OnError(Exception exception);
        void OnDispose();
    }

    public class InternalObserverException : Exception
    {
        public InternalObserverException(string message) : base(message) { }
    }

    public sealed class Observer<T> : IObserver<T> where T : IObservableEventArgs
    {
        public Action<T> onNext { get; }
        public Action<Exception> onError { get; }
        public Action onDispose { get; }

        private bool _disposed;

        private string name;

        public Observer(Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, string name = default)
        {
            this.onNext = onNext;
            this.onError = onError;
            this.onDispose = onDispose;
            this.name = name;
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
                if (exc is not InternalObserverException)
                    exc = new InternalObserverException($"OnNext encountered an exception: {exc.Message}\n{exc.StackTrace}");

                if (onError != null)
                {
                    onError(exc);
                    return;
                }

                throw;
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

            throw exception;
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