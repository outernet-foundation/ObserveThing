using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableListBase<T> : Observable<IListOperation<T>>
    {
        private class ListOperation : IListOperation<T>
        {
            public IObservable<IOperation> source { get; set; }
            public OpType opType { get; set; }
            public uint elementId { get; set; }
            public int index { get; set; }
            public T value { get; set; }

            private Action<ListOperation> _handleOperationDeallocated;

            public ListOperation(Action<ListOperation> handleOperationDeallocated)
            {
                _handleOperationDeallocated = handleOperationDeallocated;
            }

            public IOperation AllocateCopy()
            {
                return new ListOperation(_handleOperationDeallocated)
                {
                    source = source,
                    opType = opType,
                    elementId = elementId,
                    index = index,
                    value = value,
                    _handleOperationDeallocated = _handleOperationDeallocated
                };
            }

            public void Deallocate()
            {
                _handleOperationDeallocated?.Invoke(this);
            }
        }

        private List<(uint id, T value)> _list = new List<(uint id, T value)>();
        private CollectionIdProvider _idProvider;
        private Stack<ListOperation> _operationPool = new Stack<ListOperation>();

        public ObservableListBase(ObservationContext context) : this(context, null) { }
        public ObservableListBase(ObservationContext context, IEnumerable<T> value) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _list.Any(item => item.id == x));

            if (value == null)
                return;

            foreach (var element in value)
                _list.Add(new(_idProvider.GetUnusedId(), element));
        }

        protected int GetCountInternal()
            => _list.Count;

        protected IEnumerable<(uint id, T value)> ElementsInternal()
            => _list;

        private ListOperation AllocateOperation(OpType opType, uint elementId, int index, T value)
        {
            if (!_operationPool.TryPop(out var operation))
                operation = new ListOperation(DeallocateOperation);

            operation.source = this;
            operation.opType = opType;
            operation.elementId = elementId;
            operation.index = index;
            operation.value = value;

            return operation;
        }

        private void DeallocateOperation(ListOperation operation)
        {
            operation.source = default;
            operation.opType = default;
            operation.elementId = default;
            operation.index = default;
            operation.value = default;

            _operationPool.Push(operation);
        }

        public override IReadOnlyList<IListOperation<T>> GetInitializationOperations()
            => _list.Select((element, index) => AllocateOperation(OpType.Add, element.id, index, element.value)).ToArray();

        protected void AddInternal(T added)
            => InsertInternal(_list.Count, added);

        protected void AddRangeInternal(IEnumerable<T> toAdd)
        {
            foreach (var added in toAdd)
                AddInternal(added);
        }

        protected bool RemoveInternal(T removed)
        {
            var index = _list.FindIndex(x => Equals(x.value, removed));

            if (index == -1)
                return false;

            RemoveAtInternal(index);
            return true;
        }

        protected void RemoveAtInternal(int index)
        {
            var removed = _list[index];
            _list.RemoveAt(index);
            EnqueuePendingOperation(AllocateOperation(OpType.Remove, removed.id, index, removed.value));
        }

        protected void InsertInternal(int index, T item)
        {
            (uint id, T value) inserted = new(_idProvider.GetUnusedId(), item);
            _list.Insert(index, inserted);
            EnqueuePendingOperation(AllocateOperation(OpType.Add, inserted.id, index, inserted.value));
        }

        protected void ClearInternal()
        {
            while (_list.Count > 0)
                RemoveAtInternal(_list.Count - 1);
        }

        protected T ElementAtInternal(int index)
            => _list[index].value;

        protected (uint id, T value) ElementAndIdAtInternal(int index)
            => _list[index];

        protected int IndexOfInternal(T item)
            => _list.FindIndex(x => Equals(x.value, item));

        protected bool ContainsInternal(T item)
            => _list.Any(x => Equals(x.value, item));
    }
}