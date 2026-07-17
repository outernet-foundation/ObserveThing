using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class BatchObservable<T> : IInitializationOperationsProvider<IReadOnlyList<T>>, IPendingObserver
    {
        public bool immediate { get; } = false;
        public uint priority { get; private set; }
        public bool disposed { get; private set; }

        private IObservable<T> _source;
        private IObservableOperand<IReadOnlyList<T>> _operand;
        private IDisposable _subscriptions;
        private bool _pending;

        private List<T> _batchedOperations = new List<T>();

        public BatchObservable(IObservable<T> source, IObservableOperand<IReadOnlyList<T>> operand)
        {
            priority = source.context.AllocateObserverPriority();

            _source = source;
            _operand = operand;

            _subscriptions = source.Subscribe(
                onNext: HandleSourceOperation,
                onError: operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        private void HandleSourceOperation(T operation)
        {
            _batchedOperations.Add(operation);

            if (_pending)
                return;

            _pending = true;
            _source.context.RegisterPendingObserver(this);
            _source.context.NotifyPendingObserversIfNecessary();
        }

        public IReadOnlyList<IReadOnlyList<T>> GetInitializationOperations()
            => new IReadOnlyList<T>[] { _source.GetInitializationOperations() };

        public void SendNext()
        {
            _pending = false;

            var operation = _batchedOperations.ToArray();
            _batchedOperations.Clear();

            _operand.EnqueuePendingOperation(operation);
        }

        public void Dispose()
        {
            disposed = true;
            _source.context.DeallocateObserverPriority(priority);
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }
}