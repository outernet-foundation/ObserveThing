using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct DictionaryOpData<TKey, TValue>
    {
        public uint id;
        public KeyValuePair<TKey, TValue> kvp;
        public bool isRemove;
    }

    public class DictionaryObservable<TKey, TValue> : IDictionaryObservable<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
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

        public IEnumerable<TKey> keys => _dictionary.Keys;
        public IEnumerable<TValue> values => _dictionary.Values.Select(x => x.value);

        public int count => _dictionary.Count;

        private Dictionary<TKey, (uint id, TValue value)> _dictionary = new Dictionary<TKey, (uint id, TValue value)>();
        private SynchronizedNotificationQueue<IDictionaryObserver<TKey, TValue>, DictionaryOpData<TKey, TValue>> _notificationQueue;
        private CollectionIdProvider _idProvider;

        public DictionaryObservable(params KeyValuePair<TKey, TValue>[] source) : this(source, default) { }
        public DictionaryObservable(SynchronizationContext context, params KeyValuePair<TKey, TValue>[] source) : this(source, context) { }
        public DictionaryObservable(IEnumerable<KeyValuePair<TKey, TValue>> source, SynchronizationContext context = default) : this(context)
        {
            foreach (var kvp in source)
                _dictionary.Add(kvp.Key, new(_idProvider.GetUnusedId(), kvp.Value));
        }

        public DictionaryObservable(SynchronizationContext context = default)
        {
            _notificationQueue = new SynchronizedNotificationQueue<IDictionaryObserver<TKey, TValue>, DictionaryOpData<TKey, TValue>>(NotifyObserver, context);
            _idProvider = new CollectionIdProvider(x => _dictionary.Values.Any(y => y.id == x));
        }

        private void NotifyObserver(IDictionaryObserver<TKey, TValue> observer, DictionaryOpData<TKey, TValue> data)
        {
            if (data.isRemove)
            {
                observer.OnRemove(data.id, data.kvp);
            }
            else
            {
                observer.OnAdd(data.id, data.kvp);
            }
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
            _notificationQueue.EnqueueNotify(new() { id = id, kvp = new(key, value), isRemove = false });
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            _notificationQueue.EnqueueNotify(new() { id = data.id, kvp = new(key, data.value), isRemove = true });

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                _notificationQueue.EnqueueNotify(new() { id = kvp.Value.id, kvp = new(kvp.Key, kvp.Value.value), isRemove = true });
            }
        }

        public bool ContainsKey(TKey key)
            => _dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value)
            => _dictionary.Values.Select(x => x.value).Contains(value);

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer)
        {
            var subscription = _notificationQueue.AddObserver(observer);

            foreach (var kvp in _dictionary)
                observer.OnAdd(kvp.Value.id, new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.value));

            return subscription;
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
            _notificationQueue.Dispose();
        }
    }
}