using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public static class Settings
    {
        public static Action<Exception> DefaultExceptionHandler = UnityEngine.Debug.LogException;
        public static ObservationContext DefaultObservationContext = new ObservationContext();
    }

    public enum OpType
    {
        Add,
        Remove
    }

    public interface IObserver<in T>
    {
        uint? overridePriority { get; }
        bool immediate { get; }
        void OnNext(T operation);
        void OnError(Exception exc);
        void OnDispose();
    }

    public class Observer<T> : IObserver<T>
    {
        public uint? overridePriority { get; }
        public bool immediate { get; }
        private Action<T> _onNext;
        private Action<Exception> _onError;
        private Action _onDispose;

        public Observer(Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, uint? overridePriority = default, bool immediate = false)
        {
            _onNext = onNext;
            _onError = onError;
            _onDispose = onDispose;
            this.overridePriority = overridePriority;
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