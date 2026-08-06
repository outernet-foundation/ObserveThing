using System;
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
        public IObservable source { get; set; }
        public uint elementId { get; set; }
        public TKey key { get; set; }
        public TValue value { get; set; }
        public bool isRemove { get; set; }

        object IDictionaryOp.key => key;
        object IDictionaryOp.value => value;
    }

    public class ObservableDictionaryBase<TKey, TValue> : ObservableBase<IDictionaryObserver<TKey, TValue>, DictionaryOp<TKey, TValue>>, IDictionaryObservable<TKey, TValue>
    {
        private Dictionary<TKey, (uint id, TValue value)> _dictionary = new Dictionary<TKey, (uint id, TValue value)>();
        private CollectionIdProvider _idProvider;

        public ObservableDictionaryBase(ObservationContext context) : this(context, null) { }
        public ObservableDictionaryBase(ObservationContext context, IEnumerable<KeyValuePair<TKey, TValue>> value) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _dictionary.Values.Any(y => y.id == x));

            if (value == null)
                return;

            foreach (var kvp in value)
                _dictionary.Add(kvp.Key, new(_idProvider.GetUnusedId(), kvp.Value));
        }

        protected int GetCountInternal()
            => _dictionary.Count;

        protected IEnumerable<TKey> GetKeysInternal()
            => _dictionary.Keys;

        protected IEnumerable<TValue> GetValuesInternal()
            => _dictionary.Values.Select(x => x.value);

        protected IEnumerable<KeyValuePair<TKey, (uint id, TValue value)>> ElementsInternal()
            => _dictionary;

        protected void SetInternal(TKey key, TValue value)
        {
            RemoveInternal(key);
            AddInternal(key, value);
        }

        protected void AddInternal(TKey key, TValue value)
        {
            var id = _idProvider.GetUnusedId();
            _dictionary.Add(key, (id, value));
            EnqueuePendingOperation(new DictionaryOp<TKey, TValue>() { source = this, elementId = id, key = key, value = value, isRemove = false });
        }

        protected bool RemoveInternal(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            EnqueuePendingOperation(new DictionaryOp<TKey, TValue>() { source = this, elementId = data.id, key = key, value = data.value, isRemove = true });

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                EnqueuePendingOperation(new DictionaryOp<TKey, TValue>() { source = this, elementId = kvp.Value.id, key = kvp.Key, value = kvp.Value.value, isRemove = true });
            }
        }

        public TValue GetValue(TKey key)
            => _dictionary[key].value;

        protected (uint id, TValue value) GetValueWithIdInternal(TKey key)
            => _dictionary[key];

        protected bool TryGetValueInternal(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out var data))
            {
                value = data.value;
                return true;
            }

            value = default;
            return false;
        }

        protected bool TryGetValueWithIdInternal(TKey key, out (uint id, TValue value) valueWithId)
            => _dictionary.TryGetValue(key, out valueWithId);

        protected bool ContainsKeyInternal(TKey key)
            => _dictionary.ContainsKey(key);

        protected bool ContainsValueInternal(TValue value)
            => _dictionary.Values.Select(x => x.value).Contains(value);

        protected override IEnumerable<DictionaryOp<TKey, TValue>> GetInitializationOperations()
        {
            foreach (var kvp in _dictionary)
            {
                yield return new DictionaryOp<TKey, TValue>()
                {
                    source = this,
                    elementId = kvp.Value.id,
                    key = kvp.Key,
                    value = kvp.Value.value,
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

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer, bool immediate = false, uint? priority = null)
            => AddObserver(observer, immediate, priority);

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