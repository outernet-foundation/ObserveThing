using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObserveThing
{
    public class ObservableValueBase<T> : Observable<IOperation<T>>
    {
        private class Operation : IOperation<T>
        {
            public IObservable<IOperation> source { get; set; }
            public T value { get; set; }

            private Action<Operation> _handleOperationDeallocated;

            public Operation(Action<Operation> handleOperationDeallocated)
            {
                _handleOperationDeallocated = handleOperationDeallocated;
            }

            public IOperation AllocateCopy()
            {
                return new Operation(_handleOperationDeallocated)
                {
                    source = source,
                    value = value,
                    _handleOperationDeallocated = _handleOperationDeallocated
                };
            }

            public void Deallocate()
            {
                _handleOperationDeallocated?.Invoke(this);
            }

            public override string ToString()
            {
                return $"Operation[{source} : {value}]";
            }
        }

        protected T _value { get; private set; }
        private IOperation<T>[] _initOperations = new IOperation<T>[1];
        private Stack<Operation> _operationPool = new Stack<Operation>();

        public ObservableValueBase(ObservationContext context) : this(context, default) { }
        public ObservableValueBase(ObservationContext context, T value) : base(context)
        {
            _value = value;
        }

        private Operation AllocateOperation(T value)
        {
            if (!_operationPool.TryPop(out var operation))
                operation = new Operation(DeallocateOperation);

            operation.source = this;
            operation.value = value;

            return operation;
        }

        private void DeallocateOperation(Operation operation)
        {
            operation.source = default;
            operation.value = default;

            _operationPool.Push(operation);
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