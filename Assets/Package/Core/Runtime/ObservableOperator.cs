using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableOperator<T> : Observable<T>, IObservableOperand<T>
    {
        IObservable<T> IObservableOperand<T>.operationSource => this;

        private Func<IObservableOperand<T>, IInitializationOperationsProvider<T>> _generateOperator;
        private IInitializationOperationsProvider<T> _operator;
        private bool _active = false;

        public ObservableOperator(ObservationContext context, Func<IObservableOperand<T>, IInitializationOperationsProvider<T>> generateOperator) : base(context)
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

        public override IReadOnlyList<T> GetInitializationOperations()
            => _operator.GetInitializationOperations();

        void IObservableOperand<T>.EnqueuePendingOperation(T operation)
            => EnqueuePendingOperation(operation);

        void IOperand.OnError(Exception error)
            => OnError(error);

        void IOperand.OnDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }
    }
}