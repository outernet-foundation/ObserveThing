using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class BatchOperator : ObservableBase<IBatchObserver, IReadOnlyList<IOperation>>, IBatchOperand, IBatchObservable
    {
        IBatchObservable IBatchOperand.operationSource => this;

        private Func<IBatchOperand, IInitializationOperationsProvider> _generateOperator;
        private IInitializationOperationsProvider _operator;
        private Stack<Operation> _operations = new Stack<Operation>();
        private bool _active = false;

        public BatchOperator(ObservationContext context, Func<IBatchOperand, IInitializationOperationsProvider> generateOperator) : base(context)
        {
            _generateOperator = generateOperator;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _operator = _generateOperator.Invoke(this);
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            _operator?.Dispose();
            _operator = null;
        }

        protected override void DisposeInternal()
        {
            _operator?.Dispose();
            _operator = null;
        }

        void IBatchOperand.EnqueuePendingOperation(IReadOnlyList<IOperation> operation)
            => EnqueuePendingOperation(operation);

        void IOperand.OnError(Exception error)
            => OnError(error);

        void IOperand.OnDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        protected override void SendOperation(IBatchObserver observer, IReadOnlyList<IOperation> operation)
            => observer.OnNext(operation);

        public IDisposable Subscribe(IBatchObserver observer, bool immediate = false, uint? priority = null)
        {
            var subscription = AddObserver(observer, immediate, priority);
            observer.OnNext(_operator.GetInitializationOperations());
            return subscription;
        }

        public IDisposable Subscribe(IObserver observer, bool immediate = false, uint? priority = null)
            => Subscribe(new BatchObserver(
                onNext: op =>
                {
                    var operation = _operations.TryPop(out var pooledOp) ? pooledOp : new Operation(this);
                    operation.args = op;
                    observer.OnNext(operation);
                    operation.args = null;
                    _operations.Push(operation);
                }
            ), immediate, priority);
    }
}