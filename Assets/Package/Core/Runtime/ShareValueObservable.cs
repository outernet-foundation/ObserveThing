using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ShareValueObservable<T> : IValueObservable<T>
    {
        private IValueObservable<T> _source;
        private IDisposable _sourceStream;
        private T _value = default;
        private Queue<Action<IValueObserver<T>>> _pendingNotifications = new Queue<Action<IValueObserver<T>>>();
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _streamInitialized = false;
        private bool _disposed;

        private class ObserverData : IDisposable
        {
            public IValueObserver<T> observer;
            public Action<ObserverData> handleDispose;
            public bool disposed { get; private set; }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                handleDispose?.Invoke(this);
                observer.OnDispose();
            }
        }

        public ShareValueObservable(IValueObservable<T> source)
        {
            _source = source;
        }

        private void NotifyObserversOrEnqueue(Action<IValueObserver<T>> notify)
        {
            _pendingNotifications.Enqueue(notify);

            if (_notifyingObservers)
                return;

            _notifyingObservers = true;
            _streamInitialized = true;

            while (_pendingNotifications.TryDequeue(out var nextNotify))
            {
                int count = _observers.Count;
                for (int i = 0; i < count; i++)
                {
                    var instance = _observers[i];

                    if (instance.disposed)
                        continue;

                    nextNotify(instance.observer);
                }

                foreach (var disposed in _disposedObservers)
                    _observers.Remove(disposed);

                _disposedObservers.Clear();

                if (_observers.Count == 0)
                {
                    _streamInitialized = false;
                    _sourceStream?.Dispose();
                    _sourceStream = null;
                }
            }

            _notifyingObservers = false;
        }

        private void HandleObserverDisposed(ObserverData observer)
        {
            if (_disposed)
                return;

            if (_notifyingObservers)
            {
                _disposedObservers.Add(observer);
                return;
            }

            _observers.Remove(observer);

            if (_observers.Count == 0)
            {
                _streamInitialized = false;
                _sourceStream?.Dispose();
                _sourceStream = null;
            }
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
        {
            var data = new ObserverData() { observer = observer, handleDispose = HandleObserverDisposed };
            _observers.Add(data);

            if (_observers.Count == 1)
            {
                _sourceStream = _source.Subscribe(
                    onNext: x =>
                    {
                        _value = x;
                        NotifyObserversOrEnqueue(observer => observer.OnNext(x));
                    },
                    onError: x => NotifyObserversOrEnqueue(observer => observer.OnError(x)),
                    onDispose: Dispose
                );

                if (!_streamInitialized)
                    data.observer.OnNext(_value);
            }
            else
            {
                data.observer.OnNext(_value);
            }

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