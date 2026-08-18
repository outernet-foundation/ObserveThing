using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IBatchOp<T> : IOperation where T : IOperation
    {
        IReadOnlyList<T> operations { get; set; }
    }

    public struct BatchOp<T> : IBatchOp<T> where T : IOperation
    {
        public IObservable<IOperation> source { get; set; }
        public IReadOnlyList<T> operations { get; set; }
    }

    public class BatchOperator<T> : ObservableBase<IBatchObserver<T>, BatchOp<T>>, IBatchOperand<T>, IBatchObservable<T> where T : IOperation
    {
        private Func<IBatchOperand<T>, IInitializationOperationsProvider<T>> _generateOperator;
        private IInitializationOperationsProvider<T> _operator;
        private bool _active = false;

        public BatchOperator(ObservationContext context, Func<IBatchOperand<T>, IInitializationOperationsProvider<T>> generateOperator) : base(context)
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

        void IBatchOperand<T>.EnqueuePendingOperation(IReadOnlyList<T> operation)
            => EnqueuePendingOperation(new() { source = this, operations = operation });

        void IOperand.OnError(Exception error)
            => OnError(error);

        void IOperand.OnDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        protected override IEnumerable<BatchOp<T>> GetInitializationOperations()
        {
            yield return new BatchOp<T>() { source = this, operations = _operator.GetInitializationOperations() };
        }

        protected override void SendOperation(IBatchObserver<T> observer, BatchOp<T> operation)
            => observer.OnNext(operation.operations);
    }
}