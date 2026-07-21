using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableValueBase<T> : Observable<IOperation<T>>
    {
        private class Operation : IOperation<T>
        {
            public IObservable<IOperation> source { get; set; }
            public T value { get; set; }

            private OperationPool<Operation> _pool;

            public Operation(OperationPool<Operation> pool)
            {
                _pool = pool;
            }

            public IOperation Duplicate()
            {
                var duplicate = _pool.Allocate();
                duplicate.source = source;
                duplicate.value = value;
                return duplicate;
            }

            public void Dispose()
            {
                source = default;
                value = default;
                _pool.Deallocate(this);
            }
        }

        protected T _value { get; private set; }
        private IOperation<T>[] _initOperations = new IOperation<T>[1];
        private OperationPool<Operation> _operationPool;

        public ObservableValueBase(ObservationContext context) : this(context, default) { }
        public ObservableValueBase(ObservationContext context, T value) : base(context)
        {
            _operationPool = new OperationPool<Operation>(pool => new Operation(pool));
            _value = value;
        }

        private Operation AllocateOperation(T value)
        {
            var operation = _operationPool.Allocate();

            operation.source = this;
            operation.value = value;

            return operation;
        }

        public override IReadOnlyList<IOperation<T>> GetInitializationOperations()
        {
            _initOperations[0] = AllocateOperation(_value);
            return _initOperations;
        }

        protected void SetValueInternal(T value)
        {
            if (Equals(_value, value))
                return;

            _value = value;
            EnqueuePendingOperation(AllocateOperation(value));
        }
    }
}