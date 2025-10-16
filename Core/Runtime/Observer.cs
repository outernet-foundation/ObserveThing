using System;

namespace ObserveThing
{
    public interface IObserver
    {
        void OnNext(ObservableEventArgs args);
    }

    public interface IObserver<in T> : IObserver where T : ObservableEventArgs
    {
        void OnNext(T args);
        void OnError(Exception exception);
        void OnDispose();

        void IObserver.OnNext(ObservableEventArgs args)
            => OnNext((T)args);
    }

    public sealed class Observer<T> : IObserver<T> where T : ObservableEventArgs
    {
        public Action<T> onNext;
        public Action<Exception> onError;
        public Action onDispose;

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