using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableSetBase<T> : Observable<ISetOperation<T>>
    {
        private class SetOperation : ISetOperation<T>
        {
            public IObservable<IOperation> source { get; set; }
            public OpType opType { get; set; }
            public uint elementId { get; set; }
            public T value { get; set; }

            private Action<SetOperation> _handleOperationDeallocated;

            public SetOperation(Action<SetOperation> handleOperationDeallocated)
            {
                _handleOperationDeallocated = handleOperationDeallocated;
            }

            public IOperation AllocateCopy()
            {
                return new SetOperation(_handleOperationDeallocated)
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

        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private CollectionIdProvider _idProvider;
        private Stack<SetOperation> _operationPool = new Stack<SetOperation>();

        public ObservableSetBase(ObservationContext context) : this(context, null) { }
        public ObservableSetBase(ObservationContext context, IEnumerable<T> values) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _set.ContainsValue(x));

            if (values == null)
                return;

            foreach (T value in values)
                _set.Add(value, _idProvider.GetUnusedId());
        }

        private SetOperation AllocateOperation(OpType opType, uint elementId, T value)
        {
            if (!_operationPool.TryPop(out var operation))
                operation = new SetOperation(DeallocateOperation);

            operation.source = this;
            operation.opType = opType;
            operation.elementId = elementId;
            operation.value = value;

            return operation;
        }

        private void DeallocateOperation(SetOperation operation)
        {
            operation.source = default;
            operation.opType = default;
            operation.elementId = default;
            operation.value = default;

            _operationPool.Push(operation);
        }

        public override IReadOnlyList<ISetOperation<T>> GetInitializationOperations()
            => _set.Select(x => AllocateOperation(OpType.Add, x.Value, x.Key)).ToArray();

        protected int GetCountInternal()
            => _set.Count;

        protected IEnumerable<KeyValuePair<T, uint>> GetElementsInternal()
            => _set;

        protected bool AddInternal(T element)
        {
            if (_set.ContainsKey(element))
                return false;

            var id = _idProvider.GetUnusedId();
            _set.Add(element, id);
            EnqueuePendingOperation(AllocateOperation(OpType.Add, id, element));
            return true;
        }

        protected void AddRangeInternal(IEnumerable<T> elements)
        {
            foreach (var element in elements)
                AddInternal(element);
        }

        protected bool RemoveInternal(T element)
        {
            if (!_set.TryGetValue(element, out var id))
                return false;

            _set.Remove(element);
            EnqueuePendingOperation(AllocateOperation(OpType.Remove, id, element));

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                EnqueuePendingOperation(AllocateOperation(OpType.Remove, kvp.Value, kvp.Key));
            }
        }

        protected bool ContainsInternal(T element)
            => _set.ContainsKey(element);
    }
}