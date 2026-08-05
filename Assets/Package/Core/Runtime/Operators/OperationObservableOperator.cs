using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class OperationObservableOperator : ObservableBase<IObserver, IOperation>, IOperationObservableOperand, IObservable
    {
        IObservable IOperationObservableOperand.operationSource => this;

        private Func<IOperationObservableOperand, IInitializationOperationsProvider> _generateOperator;
        private IInitializationOperationsProvider _operator;
        private bool _active = false;

        public OperationObservableOperator(ObservationContext context, Func<IOperationObservableOperand, IInitializationOperationsProvider> generateOperator) : base(context)
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

        void IOperationObservableOperand.EnqueuePendingOperation(IOperation operation)
            => EnqueuePendingOperation(operation);

        void IOperand.OnError(Exception error)
            => OnError(error);

        void IOperand.OnDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        protected override void SendOperation(IObserver observer, IOperation operation)
            => observer.OnNext(operation);

        public IDisposable Subscribe(IObserver observer, bool immediate = false, uint? priority = null)
        {
            var subscription = AddObserver(observer, immediate, priority);
            foreach (var op in _operator.GetInitializationOperations())
                observer.OnNext(op);
            return subscription;
        }
    }
}