using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ValueObservable<T> : IValueObservable<T>
    {
        public T value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                _value = value;

                foreach (var observer in SafeObserverEnumeration())
                    observer.OnNext(value);
            }
        }

        private T _value = default;
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _executingSafeEnumerate;
        private bool _disposed;

        private class ObserverData : IDisposable
        {
            public IValueObserver<T> observer;
            public Action<ObserverData> onDispose;
            public bool disposed { get; private set; }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                onDispose?.Invoke(this);
                observer.OnDispose();
            }
        }

        public ValueObservable() : this(default) { }
        public ValueObservable(T startValue)
        {
            _value = startValue;
        }

        private IEnumerable<IValueObserver<T>> SafeObserverEnumeration()
        {
            if (_executingSafeEnumerate)
                throw new Exception("Cannot apply changes while already applying changes");

            _executingSafeEnumerate = true;

            int count = _observers.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _observers[i];
                if (instance.disposed)
                    continue;

                yield return instance.observer;
            }

            _executingSafeEnumerate = true;

            foreach (var disposed in _disposedObservers)
                _observers.Remove(disposed);
        }

        private void HandleObserverDisposed(ObserverData observer)
        {
            if (_disposed)
                return;

            if (_executingSafeEnumerate)
            {
                _disposedObservers.Add(observer);
                return;
            }

            _observers.Remove(observer);
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
        {
            var data = new ObserverData() { observer = observer, onDispose = HandleObserverDisposed };
            _observers.Add(data);
            data.observer.OnNext(value);
            return data;
        }

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ValueObserver<T>(
                onNext: _ => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var instance in _observers)
                instance.Dispose();

            _observers.Clear();
        }
    }
}