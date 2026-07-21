using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableCollectionBase<T> : Observable<ICollectionOperation<T>>
    {
        private class CollectionOperation : ICollectionOperation<T>
        {
            public IObservable<IOperation> source { get; set; }
            public OpType opType { get; set; }
            public uint elementId { get; set; }
            public T value { get; set; }

            private OperationPool<CollectionOperation> _pool;

            public CollectionOperation(OperationPool<CollectionOperation> pool)
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

        private Dictionary<uint, T> _collection = new Dictionary<uint, T>();
        private OperationPool<CollectionOperation> _operationPool;

        public ObservableCollectionBase(ObservationContext context) : base(context)
        {
            _operationPool = new OperationPool<CollectionOperation>(pool => new CollectionOperation(pool));
        }

        private CollectionOperation AllocateOperation(OpType opType, uint elementId, T value)
        {
            var operation = _operationPool.Allocate();

            operation.source = this;
            operation.opType = opType;
            operation.elementId = elementId;
            operation.value = value;

            return operation;
        }

        public override IReadOnlyList<ICollectionOperation<T>> GetInitializationOperations()
            => _collection.Select(x => AllocateOperation(OpType.Add, x.Key, x.Value)).ToArray();

        protected IEnumerable<(uint id, T element)> GetElementsWithIdsInternal()
            => _collection.Select<KeyValuePair<uint, T>, (uint id, T element)>(x => new(x.Key, x.Value));

        protected int GetCountInternal() => _collection.Count;

        protected uint AddInternal(uint id, T element)
        {
            _collection.Add(id, element);
            EnqueuePendingOperation(AllocateOperation(OpType.Add, id, element));
            return id;
        }

        protected bool RemoveInternal(uint id)
        {
            if (!_collection.TryGetValue(id, out var element))
                return false;

            _collection.Remove(id);
            EnqueuePendingOperation(AllocateOperation(OpType.Remove, id, element));
            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _collection.ToArray())
            {
                _collection.Remove(kvp.Key);
                EnqueuePendingOperation(AllocateOperation(OpType.Remove, kvp.Key, kvp.Value));
            }
        }

        public bool ContainsId(uint id)
            => _collection.ContainsKey(id);

        public bool Contains(T element)
            => _collection.ContainsValue(element);
    }
}