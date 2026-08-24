using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IDictionaryOp : IOperation
    {
        uint elementId { get; }
        object key { get; }
        object value { get; }
        bool isRemove { get; }
    }

    public struct DictionaryOp<TKey, TValue> : IDictionaryOp
    {
        public IObservable<IOperation> source { get; set; }
        public uint elementId { get; set; }
        public TKey key { get; set; }
        public TValue value { get; set; }
        public bool isRemove { get; set; }

        object IDictionaryOp.key => key;
        object IDictionaryOp.value => value;
    }

    public class ObservableDictionary<TKey, TValue> : ObservableBase<IDictionaryObserver<TKey, TValue>, DictionaryOp<TKey, TValue>>, IDictionaryObservable<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                Remove(key);
                Add(key, value);
            }
        }

        public int Count => _dictionary.Count;

        public IEnumerable<TKey> Keys => _dictionary.Keys;
        public IEnumerable<TValue> Values => _dictionary.Values;

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _dictionary.GetEnumerator();

        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        private Dictionary<TKey, uint> _ids = new Dictionary<TKey, uint>();
        private CollectionIdProvider _idProvider;

        public ObservableDictionary() : this(default, default) { }
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> value) : this(default, value) { }

        public ObservableDictionary(ObservationContext context) : this(context, default) { }
        public ObservableDictionary(ObservationContext context, IEnumerable<KeyValuePair<TKey, TValue>> value) : base(context)
        {
            _idProvider = new CollectionIdProvider(_ids.Values.Contains);

            if (value == null)
                return;

            foreach (var kvp in value)
            {
                _dictionary.Add(kvp.Key, kvp.Value);
                _ids.Add(kvp.Key, _idProvider.GetUnusedId());
            }
        }

        public void Add(TKey key, TValue value)
        {
            var id = _idProvider.GetUnusedId();
            _dictionary.Add(key, value);
            _ids.Add(key, id);
            EnqueuePendingOperation(new DictionaryOp<TKey, TValue>() { source = this, elementId = id, key = key, value = value, isRemove = false });
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var value))
                return false;

            var id = _ids[key];

            _dictionary.Remove(key);
            _ids.Remove(key);
            EnqueuePendingOperation(new DictionaryOp<TKey, TValue>() { source = this, elementId = id, key = key, value = value, isRemove = true });

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                var id = _ids[kvp.Key];
                _dictionary.Remove(kvp.Key);
                _ids.Remove(kvp.Key);
                EnqueuePendingOperation(new DictionaryOp<TKey, TValue>() { source = this, elementId = id, key = kvp.Key, value = kvp.Value, isRemove = true });
            }
        }

        public bool TryGetValueInternal(TKey key, out TValue value)
            => _dictionary.TryGetValue(key, out value);

        public bool ContainsKey(TKey key)
            => _dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value)
            => _dictionary.ContainsValue(value);

        protected override IEnumerable<DictionaryOp<TKey, TValue>> GetInitializationOperations()
        {
            foreach (var kvp in _dictionary)
            {
                yield return new DictionaryOp<TKey, TValue>()
                {
                    source = this,
                    elementId = _ids[kvp.Key],
                    key = kvp.Key,
                    value = kvp.Value,
                    isRemove = false
                };
            }
        }

        protected override void SendOperation(IDictionaryObserver<TKey, TValue> observer, DictionaryOp<TKey, TValue> operation)
        {
            if (operation.isRemove)
            {
                observer.OnRemove(operation.elementId, KeyValuePair.Create(operation.key, operation.value));
            }
            else
            {
                observer.OnAdd(operation.elementId, KeyValuePair.Create(operation.key, operation.value));
            }
        }

        IDisposable IDictionaryObservable.Subscribe(IDictionaryObserver observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, kvp) => observer.OnAdd(id, new KeyValuePair<object, object>(kvp.Key, kvp.Value)),
                onRemove: (id, kvp) => observer.OnRemove(id, new KeyValuePair<object, object>(kvp.Key, kvp.Value)),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable<KeyValuePair<TKey, TValue>>.Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, kvp) => observer.OnAdd(id, kvp),
                onRemove: (id, kvp) => observer.OnRemove(id, kvp),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, kvp) => observer.OnAdd(id, kvp),
                onRemove: (id, kvp) => observer.OnRemove(id, kvp),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}