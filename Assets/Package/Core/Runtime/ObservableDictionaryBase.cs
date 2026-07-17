using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableDictionaryBase<TKey, TValue> : Observable<CollectionOp<KeyValuePair<TKey, TValue>>>
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

        public override IReadOnlyList<CollectionOp<KeyValuePair<TKey, TValue>>> GetInitializationOperations()
            => _dictionary.Select(x => new CollectionOp<KeyValuePair<TKey, TValue>>()
            {
                opType = OpType.Add,
                elementId = x.Value.id,
                value = new KeyValuePair<TKey, TValue>(x.Key, x.Value.value)
            }).ToArray();

        protected void SetInternal(TKey key, TValue value)
        {
            RemoveInternal(key);
            AddInternal(key, value);
        }

        protected void AddInternal(TKey key, TValue value)
        {
            var id = _idProvider.GetUnusedId();
            _dictionary.Add(key, (id, value));
            EnqueuePendingOperation(new CollectionOp<KeyValuePair<TKey, TValue>>()
            {
                opType = OpType.Add,
                elementId = id,
                value = new KeyValuePair<TKey, TValue>(key, value)
            });
        }

        protected bool RemoveInternal(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            EnqueuePendingOperation(new CollectionOp<KeyValuePair<TKey, TValue>>()
            {
                opType = OpType.Remove,
                elementId = data.id,
                value = new KeyValuePair<TKey, TValue>(key, data.value)
            });

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                EnqueuePendingOperation(new CollectionOp<KeyValuePair<TKey, TValue>>()
                {
                    opType = OpType.Remove,
                    elementId = kvp.Value.id,
                    value = new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.value)
                });
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
    }
}