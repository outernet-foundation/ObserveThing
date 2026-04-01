using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class DictionaryObservable<TKey, TValue> : ObservableBase<IDictionaryObserver<TKey, TValue>>, IDictionaryObservable<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
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
        private CollectionIdProvider _idProvider;

        public DictionaryObservable(SynchronizationContext context = default) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _dictionary.Values.Any(y => y.id == x));
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
            EnqueueNotify(x => x.OnAdd(id, new KeyValuePair<TKey, TValue>(key, value)));
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            EnqueueNotify(x => x.OnRemove(data.id, new KeyValuePair<TKey, TValue>(key, data.value)));

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                EnqueueNotify(x => x.OnRemove(kvp.Value.id, new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.value)));
            }
        }

        public bool ContainsKey(TKey key)
            => _dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value)
            => _dictionary.Values.Select(x => x.value).Contains(value);

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer)
        {
            var subscription = AddObserver(observer);

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
    }
}