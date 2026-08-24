using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class BatchObservable<T> : IInitializationOperationsProvider<BatchOp<T>>, IPendingObserver where T : IOperation
    {
        public bool immediate { get; } = false;
        public uint priority { get; private set; }
        public bool disposed { get; private set; }

        private IObservable<T> _source;
        private Observable<BatchOp<T>> _operand;
        private IDisposable _subscriptions;
        private bool _pending;

        private List<T> _batchedOperations = new List<T>();

        public BatchObservable(IObservable<T> source, Observable<BatchOp<T>> operand)
        {
            priority = source.context.AllocateObserverPriority();

            _source = source;
            _operand = operand;

            _subscriptions = source.Subscribe(new Observer<T>(
                onNext: HandleSourceOperation,
                onError: operand.OnError,
                onDispose: Dispose
            ), immediate: true);
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

        public IEnumerable<BatchOp<T>> GetInitializationOperations()
        {
            List<T> initOps = new List<T>();
            var subscription = _source.Subscribe(new Observer<T>(x => initOps.Add(x)));
            subscription.Dispose();
            yield return new BatchOp<T>() { source = _source, operations = initOps };
        }

        public void SendNext()
        {
            _pending = false;

            var batch = _batchedOperations.ToArray();
            _batchedOperations.Clear();

            _operand.EnqueueOperation(new BatchOp<T>() { source = _source, operations = batch });
        }

        public void Dispose()
        {
            disposed = true;
            _source.context.DeallocateObserverPriority(priority);
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }
}