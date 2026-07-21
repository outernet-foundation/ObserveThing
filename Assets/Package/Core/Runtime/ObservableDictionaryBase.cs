using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableDictionaryBase<TKey, TValue> : Observable<IDictionaryOperation<TKey, TValue>>
    {
        private class DictionaryOperation : IDictionaryOperation<TKey, TValue>
        {
            public IObservable<IOperation> source { get; set; }
            public OpType opType { get; set; }
            public uint elementId { get; set; }
            public KeyValuePair<TKey, TValue> value { get; set; }

            private OperationPool<DictionaryOperation> _pool;

            public DictionaryOperation(OperationPool<DictionaryOperation> pool)
            {
                _pool = pool;
            }

            public IOperation Duplicate()
            {
                var duplicate = _pool.Allocate();

                duplicate.source = source;
                duplicate.opType = opType;
                duplicate.elementId = elementId;
                duplicate.value = value;

                return duplicate;
            }

            public void Dispose()
            {
                source = default;
                opType = default;
                elementId = default;
                value = default;

                _pool.Deallocate(this);
            }
        }

        private Dictionary<TKey, (uint id, TValue value)> _dictionary = new Dictionary<TKey, (uint id, TValue value)>();
        private CollectionIdProvider _idProvider;
        private OperationPool<DictionaryOperation> _operationPool;

        public ObservableDictionaryBase(ObservationContext context) : this(context, null) { }
        public ObservableDictionaryBase(ObservationContext context, IEnumerable<KeyValuePair<TKey, TValue>> value) : base(context)
        {
            _operationPool = new OperationPool<DictionaryOperation>(pool => new DictionaryOperation(pool));
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

        private DictionaryOperation AllocateOperation(OpType opType, uint elementId, TKey key, TValue value)
        {
            var operation = _operationPool.Allocate();

            operation.source = this;
            operation.opType = opType;
            operation.elementId = elementId;
            operation.value = new KeyValuePair<TKey, TValue>(key, value);

            return operation;
        }

        public override IReadOnlyList<IDictionaryOperation<TKey, TValue>> GetInitializationOperations()
            => _dictionary.Select(x => AllocateOperation(OpType.Add, x.Value.id, x.Key, x.Value.value)).ToArray();

        protected void SetInternal(TKey key, TValue value)
        {
            RemoveInternal(key);
            AddInternal(key, value);
        }

        protected void AddInternal(TKey key, TValue value)
        {
            var id = _idProvider.GetUnusedId();
            _dictionary.Add(key, (id, value));
            EnqueuePendingOperation(AllocateOperation(OpType.Add, id, key, value));
        }

        protected bool RemoveInternal(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            EnqueuePendingOperation(AllocateOperation(OpType.Remove, data.id, key, data.value));

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                EnqueuePendingOperation(AllocateOperation(OpType.Remove, kvp.Value.id, kvp.Key, kvp.Value.value));
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