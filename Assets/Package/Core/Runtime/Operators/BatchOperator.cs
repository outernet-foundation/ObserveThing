using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IBatchOp : IOperation
    {
        IReadOnlyList<IOperation> operations { get; set; }
    }

    public struct BatchOp : IBatchOp
    {
        public IObservable source { get; set; }
        public IReadOnlyList<IOperation> operations { get; set; }
    }

    public class BatchOperator : ObservableBase<IBatchObserver, BatchOp>, IBatchOperand, IBatchObservable
    {
        private Func<IBatchOperand, IInitializationOperationsProvider> _generateOperator;
        private IInitializationOperationsProvider _operator;
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
            => EnqueuePendingOperation(new() { source = this, operations = operation });

        void IOperand.OnError(Exception error)
            => OnError(error);

        void IOperand.OnDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        protected override IEnumerable<BatchOp> GetInitializationOperations()
        {
            yield return new BatchOp() { source = this, operations = _operator.GetInitializationOperations() };
        }

        protected override void SendOperation(IBatchObserver observer, BatchOp operation)
            => observer.OnNext(operation.operations);

        public IDisposable Subscribe(IBatchObserver observer, bool immediate = false, uint? priority = null)
            => AddObserver(observer, immediate, priority);
    }
}