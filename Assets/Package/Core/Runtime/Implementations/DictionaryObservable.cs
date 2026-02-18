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
            get => _dictionary[key];
            set
            {
                if (!_dictionary.TryGetValue(key, out var prevValue) || Equals(value, prevValue))
                    return;

                Remove(key);
                Add(key, value);
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        public int count => _dictionary.Count;

        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _executingSafeEnumerate;
        private bool _disposed;

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

        public DictionaryObservable() { }

        private IEnumerable<IDictionaryObserver<TKey, TValue>> SafeObserverEnumeration()
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

        public bool TryGetValue(TKey key, out TValue value)
            => _dictionary.TryGetValue(key, out value);

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);

            foreach (var observer in SafeObserverEnumeration())
                observer.OnAdd(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var value))
                return false;

            _dictionary.Remove(key);

            foreach (var observer in SafeObserverEnumeration())
                observer.OnRemove(new KeyValuePair<TKey, TValue>(key, value));

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);

                foreach (var observer in SafeObserverEnumeration())
                    observer.OnRemove(kvp);
            }
        }

        public bool ContainsKey(TKey key)
            => _dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value)
            => _dictionary.ContainsValue(value);

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer)
        {
            var data = new ObserverData() { observer = observer, onDispose = HandleObserverDisposed };

            _observers.Add(data);

            foreach (var kvp in _dictionary)
                data.observer.OnAdd(kvp);

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
                onAdd: _ => observer.OnChange(),
                onRemove: _ => observer.OnChange(),
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