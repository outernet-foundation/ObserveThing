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

        private List<Operation> _batchedOperations = new List<Operation>();
        private Stack<Operation> _operationPool = new Stack<Operation>();

        public BatchObservable(IObservable source, IBatchOperand operand)
        {
            priority = source.context.AllocateObserverPriority();

            _source = source;
            _operand = operand;

            _subscriptions = source.Subscribe(new OperationObserver(
                onNext: HandleSourceOperation,
                onError: operand.OnError,
                onDispose: Dispose
            ), immediate: true);
        }

        private void HandleSourceOperation(IOperation operation)
        {
            var copy = _operationPool.TryPop(out var op) ? op : new Operation(_source);
            copy.args = operation.args;

            _batchedOperations.Add(copy);

            if (_pending)
                return;

            _pending = true;
            _source.context.RegisterPendingObserver(this);
            _source.context.NotifyPendingObserversIfNecessary();
        }

        public IReadOnlyList<IOperation> GetInitializationOperations()
        {
            List<Operation> initOps = new List<Operation>();
            var subscription = _source.Subscribe(new OperationObserver(x => initOps.Add(new Operation(_source) { args = x.args })));
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