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

            private OperationPool<SetOperation> _pool;

            public SetOperation(OperationPool<SetOperation> pool)
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

        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private CollectionIdProvider _idProvider;
        private OperationPool<SetOperation> _operationPool;

        public ObservableSetBase(ObservationContext context) : this(context, null) { }
        public ObservableSetBase(ObservationContext context, IEnumerable<T> values) : base(context)
        {
            _operationPool = new OperationPool<SetOperation>(pool => new SetOperation(pool));
            _idProvider = new CollectionIdProvider(x => _set.ContainsValue(x));

            if (values == null)
                return;

            foreach (T value in values)
                _set.Add(value, _idProvider.GetUnusedId());
        }

        private SetOperation AllocateOperation(OpType opType, uint elementId, T value)
        {
            var operation = _operationPool.Allocate();

            operation.source = this;
            operation.opType = opType;
            operation.elementId = elementId;
            operation.value = value;

            return operation;
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