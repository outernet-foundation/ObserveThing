using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableOperator<T> : ObservableBase<IObserver<T>, T>, IObservableOperand<T>, IObservable<T> where T : IOperation
    {
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

        protected override IEnumerable<T> GetInitializationOperations()
        {
            foreach (var op in _operator.GetInitializationOperations())
                yield return op;
        }

        protected override void SendOperation(IObserver<T> observer, T operation)
            => observer.OnNext(operation);

        public IDisposable Subscribe(IObserver<T> observer, bool immediate = false, uint? priority = null)
            => AddObserver(observer, immediate, priority);
    }
}