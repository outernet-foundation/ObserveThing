using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ShareSetObservable<T> : ISetObservable<T>
    {
        private ISetObservable<T> _source;
        private IDisposable _sourceStream;
        private HashSet<(uint id, T value)> _set = new HashSet<(uint id, T value)>();
        private Queue<Action<ISetObserver<T>>> _pendingNotifications = new Queue<Action<ISetObserver<T>>>();
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _disposed;

        private class ObserverData : IDisposable
        {
            public ISetObserver<T> observer;
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

        public ShareSetObservable(ISetObservable<T> source)
        {
            _source = source;
        }

        private void NotifyObserversOrEnqueue(Action<ISetObserver<T>> notify)
        {
            _pendingNotifications.Enqueue(notify);

            if (_notifyingObservers)
                return;

            _notifyingObservers = true;

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
                _sourceStream?.Dispose();
                _sourceStream = null;
            }
        }

        public IDisposable Subscribe(ISetObserver<T> observer)
        {
            var data = new ObserverData() { observer = observer, handleDispose = HandleObserverDisposed };
            _observers.Add(data);

            if (_observers.Count == 1)
            {
                _sourceStream = _source.SubscribeWithId(
                    onAdd: (id, x) =>
                    {
                        _set.Add(new(id, x));
                        NotifyObserversOrEnqueue(observer => observer.OnAdd(id, x));
                    },
                    onRemove: (id, x) =>
                    {
                        _set.Remove(new(id, x));
                        NotifyObserversOrEnqueue(observer => observer.OnRemove(id, x));
                    },
                    onError: x => NotifyObserversOrEnqueue(observer => observer.OnError(x)),
                    onDispose: Dispose
                );
            }
            else
            {
                foreach (var element in _set)
                    data.observer.OnAdd(element.id, element.value);
            }

            return data;
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new SetObserver<T>(
                onAdd: (id, value) => observer.OnAdd(id, value),
                onRemove: (id, value) => observer.OnRemove(id, value),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new SetObserver<T>(
                onAdd: (_, _) => observer.OnChange(),
                onRemove: (_, _) => observer.OnChange(),
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