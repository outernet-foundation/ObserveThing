using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class DictionaryObservable<TKey, TValue> : IDictionaryObservable<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public TValue this[TKey key]
        {
            get => _dictionary[key].value;
            set
            {
                if (!_dictionary.TryGetValue(key, out var prevValue) || Equals(value, prevValue.value))
                    return;

                Remove(key);
                Add(key, value);
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => _dictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _dictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.value)).GetEnumerator();

        public int count => _dictionary.Count;

        private Dictionary<TKey, (uint id, TValue value)> _dictionary = new Dictionary<TKey, (uint id, TValue value)>();
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _disposed;

        private CollectionIdProvider _idProvider;

        private class ObserverData : IDisposable
        {
            public IDictionaryObserver<TKey, TValue> observer;
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

        public DictionaryObservable()
        {
            _idProvider = new CollectionIdProvider(x => _dictionary.Values.Any(y => y.id == x));
        }

        private void NotifyObservers(Action<IDictionaryObserver<TKey, TValue>> notify)
        {
            if (_notifyingObservers)
                throw new Exception("Cannot notify observers while already notifying observers.");

            _notifyingObservers = true;

            int count = _observers.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _observers[i];

                if (instance.disposed)
                    continue;

                try
                {
                    notify(instance.observer);
                }
                catch (Exception exc)
                {
                    // TODO: Decide
                    // Should errors thrown by observers be looped back into the observer's onError callback?
                    instance.observer.OnError(exc);
                }
            }

            _notifyingObservers = false;

            foreach (var disposed in _disposedObservers)
                _observers.Remove(disposed);
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
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out var data))
            {
                value = data.value;
                return true;
            }

            value = default;
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            var id = _idProvider.GetUnusedId();
            _dictionary.Add(key, (id, value));
            NotifyObservers(x => x.OnAdd(id, new KeyValuePair<TKey, TValue>(key, value)));
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            NotifyObservers(x => x.OnRemove(data.id, new KeyValuePair<TKey, TValue>(key, data.value)));

            return true;
        }

        public void Clear()
        {
            foreach (var key in _dictionary.Keys.ToArray())
                Remove(key);
        }

        public bool ContainsKey(TKey key)
            => _dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value)
            => _dictionary.Values.Select(x => x.value).Contains(value);

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer)
        {
            var data = new ObserverData() { observer = observer, onDispose = HandleObserverDisposed };

            _observers.Add(data);

            foreach (var kvp in _dictionary)
                data.observer.OnAdd(kvp.Value.id, new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.value));

            return data;
        }

        public IDisposable Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: observer.OnAdd,
                onRemove: observer.OnRemove,
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