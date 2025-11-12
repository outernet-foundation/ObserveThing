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

        public Observer(Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
        {
            this.onNext = onNext;
            this.onError = onError ?? UnityEngine.Debug.LogError;
            this.onDispose = onDispose;
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