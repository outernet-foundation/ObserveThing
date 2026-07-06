using System;

namespace ObserveThing
{
    public static class Settings
    {
        public static Action<Exception> DefaultExceptionHandler = UnityEngine.Debug.LogException;
        public static ObservationContext DefaultObservationContext = new ObservationContext();
    }

    public interface IObserver
    {
        bool immediate { get; }
        void OnNext(object args);
        void OnError(Exception exc);
        void OnDispose();
    }

    public class Observer : IObserver
    {
        public bool immediate { get; }
        private Action<object> _onNext;
        private Action<Exception> _onError;
        private Action _onDispose;

        public Observer(Action<object> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
        {
            _onNext = onNext;
            _onError = onError;
            _onDispose = onDispose;
            this.immediate = immediate;
        }

        public void OnNext(object onNext)
        {
            try
            {
                _onNext?.Invoke(onNext);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnDispose() => _onDispose?.Invoke();
        public void OnError(Exception error) => (_onError ?? Settings.DefaultExceptionHandler)?.Invoke(error);
    }

    public interface IObserver<in T>
    {
        bool immediate { get; }
        void OnNext(T args);
        void OnError(Exception exc);
        void OnDispose();
    }

    public class Observer<T> : IObserver<T>
    {
        public bool immediate { get; }
        private Action<T> _onNext;
        private Action<Exception> _onError;
        private Action _onDispose;

        public Observer(Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
        {
            _onNext = onNext;
            _onError = onError;
            _onDispose = onDispose;
            this.immediate = immediate;
        }

        public void OnNext(T args)
        {
            try
            {
                _onNext?.Invoke(args);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnDispose() => _onDispose?.Invoke();
        public void OnError(Exception error) => (_onError ?? Settings.DefaultExceptionHandler)?.Invoke(error);
    }
}