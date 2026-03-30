using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ShareListObservable<T> : IListObservable<T>
    {
        private IListObservable<T> _source;
        private IDisposable _sourceStream;
        private List<(uint id, T value)> _list = new List<(uint id, T value)>();
        private Queue<Action<IListObserver<T>>> _pendingNotifications = new Queue<Action<IListObserver<T>>>();
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _disposed;

        private class ObserverData : IDisposable
        {
            public IListObserver<T> observer;
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

        public ShareListObservable(IListObservable<T> source)
        {
            _source = source;
        }

        private void NotifyObserversOrEnqueue(Action<IListObserver<T>> notify)
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

        public IDisposable Subscribe(IListObserver<T> observer)
        {
            var data = new ObserverData() { observer = observer, handleDispose = HandleObserverDisposed };
            _observers.Add(data);

            if (_observers.Count == 1)
            {
                _sourceStream = _source.SubscribeWithId(
                    onAdd: (id, index, x) =>
                    {
                        _list.Insert(index, new(id, x));
                        NotifyObserversOrEnqueue(observer => observer.OnAdd(id, index, x));
                    },
                    onRemove: (id, index, x) =>
                    {
                        _list.RemoveAt(index);
                        NotifyObserversOrEnqueue(observer => observer.OnRemove(id, index, x));
                    },
                    onError: x => NotifyObserversOrEnqueue(observer => observer.OnError(x)),
                    onDispose: Dispose
                );
            }
            else
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    var element = _list[i];
                    data.observer.OnAdd(element.id, i, element.value);
                }
            }

            return data;
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, _, value) => observer.OnAdd(id, value),
                onRemove: (id, _, value) => observer.OnRemove(id, value),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (_, _, _) => observer.OnChange(),
                onRemove: (_, _, _) => observer.OnChange(),
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