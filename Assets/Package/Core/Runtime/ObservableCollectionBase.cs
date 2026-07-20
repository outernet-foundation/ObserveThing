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

            private Action<CollectionOperation> _handleOperationDeallocated;

            public CollectionOperation(Action<CollectionOperation> handleOperationDeallocated)
            {
                _handleOperationDeallocated = handleOperationDeallocated;
            }

            public IOperation AllocateCopy()
            {
                return new CollectionOperation(_handleOperationDeallocated)
                {
                    source = source,
                    opType = opType,
                    elementId = elementId,
                    value = value,
                    _handleOperationDeallocated = _handleOperationDeallocated
                };
            }

            public void Deallocate()
            {
                _handleOperationDeallocated?.Invoke(this);
            }
        }

        private Dictionary<uint, T> _collection = new Dictionary<uint, T>();
        private Stack<CollectionOperation> _operationPool = new Stack<CollectionOperation>();

        public ObservableCollectionBase(ObservationContext context) : base(context) { }

        private CollectionOperation AllocateOperation(OpType opType, uint elementId, T value)
        {
            if (!_operationPool.TryPop(out var operation))
                operation = new CollectionOperation(DeallocateOperation);

            operation.source = this;
            operation.opType = opType;
            operation.elementId = elementId;
            operation.value = value;

            return operation;
        }

        private void DeallocateOperation(CollectionOperation operation)
        {
            operation.source = default;
            operation.opType = default;
            operation.elementId = default;
            operation.value = default;

            _operationPool.Push(operation);
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