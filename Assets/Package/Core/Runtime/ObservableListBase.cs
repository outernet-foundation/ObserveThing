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

            private OperationPool<ListOperation> _pool;

            public ListOperation(OperationPool<ListOperation> pool)
            {
                _pool = pool;
            }

            public IOperation Duplicate()
            {
                var duplicate = _pool.Allocate();
                duplicate.source = source;
                duplicate.opType = opType;
                duplicate.elementId = elementId;
                duplicate.index = index;
                duplicate.value = value;
                return duplicate;
            }

            public void Dispose()
            {
                source = default;
                opType = default;
                elementId = default;
                index = default;
                value = default;
                _pool.Deallocate(this);
            }
        }

        private List<(uint id, T value)> _list = new List<(uint id, T value)>();
        private CollectionIdProvider _idProvider;
        private OperationPool<ListOperation> _operationPool;

        public ObservableListBase(ObservationContext context) : this(context, null) { }
        public ObservableListBase(ObservationContext context, IEnumerable<T> value) : base(context)
        {
            _operationPool = new OperationPool<ListOperation>(pool => new ListOperation(pool));
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
            var operation = _operationPool.Allocate();

            operation.source = this;
            operation.opType = opType;
            operation.elementId = elementId;
            operation.index = index;
            operation.value = value;

            return operation;
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