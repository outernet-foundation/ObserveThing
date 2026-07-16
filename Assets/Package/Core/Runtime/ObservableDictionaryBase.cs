using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IDictionaryOperation : ICollectionOperation
    {
        object key { get; }
        object value { get; }
    }

    public interface IDictionaryOperation<TKey, TValue> : IDictionaryOperation, ICollectionOperation<KeyValuePair<TKey, TValue>>
    {
        new TKey key { get; }
        new TValue value { get; }

        object IDictionaryOperation.key => key;
        object IDictionaryOperation.value => value;
    }

    public class ObservableDictionaryBase<TKey, TValue> : Observable<IDictionaryOperation<TKey, TValue>>, IDictionaryObservable<TKey, TValue>
    {
        private class DictionaryOperation : IDictionaryOperation<TKey, TValue>
        {
            public IObservable source { get; set; }
            public uint elementId { get; set; }
            public OpType opType { get; set; }
            public TKey key { get; set; }
            public TValue value { get; set; }
            public KeyValuePair<TKey, TValue> element => new KeyValuePair<TKey, TValue>(key, value);

            public IOperation AllocateCopy()
            {
                return new DictionaryOperation()
                {
                    source = source,
                    elementId = elementId,
                    opType = opType,
                    key = key,
                    value = value,
                };
            }

            public void Deallocate()
            {
                var context = source.context;
                source = default;
                elementId = default;
                opType = default;
                key = default;
                value = default;
                context.DeallocateOperation(this);
            }
        }

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

        private DictionaryOperation AllocateOperation(uint id, OpType opType, TKey key, TValue value)
        {
            var op = context.AllocateOperation<DictionaryOperation>();
            op.source = this;
            op.elementId = id;
            op.opType = opType;
            op.key = key;
            op.value = value;
            return op;
        }

        protected int GetCountInternal()
            => _dictionary.Count;

        protected IEnumerable<TKey> GetKeysInternal()
            => _dictionary.Keys;

        protected IEnumerable<TValue> GetValuesInternal()
            => _dictionary.Values.Select(x => x.value);

        protected IEnumerable<KeyValuePair<TKey, (uint id, TValue value)>> ElementsInternal()
            => _dictionary;

        public override IReadOnlyList<IDictionaryOperation<TKey, TValue>> GetInitializationOperations()
            => _dictionary.Select(x => AllocateOperation(x.Value.id, OpType.Add, x.Key, x.Value.value)).ToArray();

        protected void SetInternal(TKey key, TValue value)
        {
            RemoveInternal(key);
            AddInternal(key, value);
        }

        protected void AddInternal(TKey key, TValue value)
        {
            var id = _idProvider.GetUnusedId();
            _dictionary.Add(key, (id, value));
            EnqueuePendingOperation(AllocateOperation(id, OpType.Add, key, value));
        }

        protected bool RemoveInternal(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            EnqueuePendingOperation(AllocateOperation(data.id, OpType.Remove, key, data.value));

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                EnqueuePendingOperation(AllocateOperation(kvp.Value.id, OpType.Remove, kvp.Key, kvp.Value.value));
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

        public IDisposable Subscribe(IObserver<ICollectionOperation<KeyValuePair<TKey, TValue>>> observer)
            => Subscribe(new Observer<IDictionaryOperation<TKey, TValue>>(
                overridePriority: observer.overridePriority,
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver<ICollectionOperation> observer)
            => Subscribe(new Observer<IDictionaryOperation<TKey, TValue>>(
                overridePriority: observer.overridePriority,
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}