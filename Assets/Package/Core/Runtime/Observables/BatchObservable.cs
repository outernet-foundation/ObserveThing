using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class BatchObservable : IInitializationOperationsProvider, IPendingObserver
    {
        public bool immediate { get; } = false;
        public uint priority { get; private set; }
        public bool disposed { get; private set; }

        private IObservable _source;
        private IBatchOperand _operand;
        private IDisposable _subscriptions;
        private bool _pending;

        private List<IOperation> _batchedOperations = new List<IOperation>();

        public BatchObservable(IObservable source, IBatchOperand operand)
        {
            priority = source.context.AllocateObserverPriority();

            _source = source;
            _operand = operand;

            _subscriptions = source.Subscribe(new Observer(
                onNext: HandleSourceOperation,
                onError: operand.OnError,
                onDispose: Dispose
            ), immediate: true);
        }

        private void HandleSourceOperation(IOperation operation)
        {
            _batchedOperations.Add(operation);

            if (_pending)
                return;

            _pending = true;
            _source.context.RegisterPendingObserver(this);
            _source.context.NotifyPendingObserversIfNecessary();
        }

        public IReadOnlyList<IOperation> GetInitializationOperations()
        {
            List<IOperation> initOps = new List<IOperation>();
            var subscription = _source.Subscribe(new Observer(x => initOps.Add(x)));
            subscription.Dispose();
            return initOps;
        }

        public void SendNext()
        {
            _pending = false;

            var batch = _batchedOperations.ToArray();
            _batchedOperations.Clear();

            _operand.EnqueuePendingOperation(batch);
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