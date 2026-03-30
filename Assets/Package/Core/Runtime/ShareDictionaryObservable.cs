using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ShareDictionaryObservable<TKey, TValue> : IDictionaryObservable<TKey, TValue>
    {
        private IDictionaryObservable<TKey, TValue> _source;
        private IDisposable _sourceStream;
        private Dictionary<TKey, (uint id, KeyValuePair<TKey, TValue> value)> _dictionary = new Dictionary<TKey, (uint id, KeyValuePair<TKey, TValue> value)>();
        private Queue<Action<IDictionaryObserver<TKey, TValue>>> _pendingNotifications = new Queue<Action<IDictionaryObserver<TKey, TValue>>>();
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _disposed;

        private class ObserverData : IDisposable
        {
            public IDictionaryObserver<TKey, TValue> observer;
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

        public ShareDictionaryObservable(IDictionaryObservable<TKey, TValue> source)
        {
            _source = source;
        }

        private void NotifyObserversOrEnqueue(Action<IDictionaryObserver<TKey, TValue>> notify)
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

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer)
        {
            var data = new ObserverData() { observer = observer, handleDispose = HandleObserverDisposed };
            _observers.Add(data);

            if (_observers.Count == 1)
            {
                _sourceStream = _source.SubscribeWithId(
                    onAdd: (id, x) =>
                    {
                        _dictionary.Add(x.Key, new(id, x));
                        NotifyObserversOrEnqueue(observer => observer.OnAdd(id, x));
                    },
                    onRemove: (id, x) =>
                    {
                        _dictionary.Remove(x.Key);
                        NotifyObserversOrEnqueue(observer => observer.OnRemove(id, x));
                    },
                    onError: x => NotifyObserversOrEnqueue(observer => observer.OnError(x)),
                    onDispose: Dispose
                );
            }
            else
            {
                foreach (var kvp in _dictionary)
                    data.observer.OnAdd(kvp.Value.id, kvp.Value.value);
            }

            return data;
        }

        public IDisposable Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, value) => observer.OnAdd(id, value),
                onRemove: (id, value) => observer.OnRemove(id, value),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
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