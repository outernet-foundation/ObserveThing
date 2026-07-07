using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IBatchOperation : IOperation
    {
        IObservable originatingSource { get; }
        IReadOnlyList<object> operations { get; }
    }

    public interface IBatchOperation<T> : IBatchOperation where T : IOperation
    {
        new IObservable<T> originatingSource { get; }
        new IReadOnlyList<T> operations { get; }

        IObservable IBatchOperation.originatingSource => originatingSource;
        IReadOnlyList<object> IBatchOperation.operations => (IReadOnlyList<object>)operations;
    }


    public class BatchObservable<T> : Observable<IBatchOperation<T>>, IPendingObserver where T : IOperation
    {
        private class BatchOperation : IBatchOperation<T>
        {
            public IObservable source { get; set; }
            public IObservable<T> originatingSource { get; set; }
            public IReadOnlyList<T> operations { get; set; }

            public IOperation Clone()
            {
                return new BatchOperation()
                {
                    source = source,
                    originatingSource = originatingSource,
                    operations = operations.Select(x => (T)x.Clone()).ToArray()
                };
            }

            public void Reset()
            {
                source = default;
                originatingSource = default;
                operations = default;
            }
        }

        public bool immediate { get; } = false;
        public uint priority { get; private set; }

        private IObservable<T> _source;
        private IDisposable _subscriptions;
        private bool _active;
        private bool _initialized;
        private bool _pending;

        private List<T> _batchedOperations = new List<T>();

        public BatchObservable(IObservable<T> source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.Subscribe(
                onOperation: HandleSourceOperation,
                onError: OnError,
                onDispose: HandleSourceDisposed,
                immediate: true
            );

            priority = _source.context.AllocateObserverPriority();
            _initialized = true;
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            context.DeallocateObserverPriority(priority);
            _subscriptions?.Dispose();
            _subscriptions = null;
            _initialized = false;
        }


        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        protected override void OnOperationNotificationsCompleted(IBatchOperation<T> operation)
        {
            var op = (BatchOperation)operation;
            op.Reset();
            context.DeallocatePooledOperation(op);
        }

        private void HandleSourceOperation(T operation)
        {
            _batchedOperations.Add((T)operation.Clone());

            if (_pending || !_initialized)
                return;

            _pending = true;
            context.RegisterPendingObserver(this);
            context.NotifyPendingObserversIfNecessary();
        }

        public void SendNext()
        {
            _pending = false;

            var operation = context.AllocatePooledOperation<BatchOperation>();

            operation.source = this;
            operation.originatingSource = _source;
            operation.operations = _batchedOperations.ToArray();

            _batchedOperations.Clear();

            EnqueuePendingOperation(operation);
        }

        protected override void DisposeInternal()
        {
            context.DeallocateObserverPriority(priority);
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        protected override IReadOnlyList<IBatchOperation<T>> GetInitializationOperations()
        {
            var operation = context.AllocatePooledOperation<BatchOperation>();

            operation.source = this;
            operation.originatingSource = _source;
            operation.operations = _batchedOperations.ToArray();

            _batchedOperations.Clear();

            return new IBatchOperation<T>[] { operation };
        }
    }
}