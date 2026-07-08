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


    public class BatchObservable<T> : IInitializationOperationsProvider<IBatchOperation<T>>, IPendingObserver where T : IOperation
    {
        private class BatchOperation : IBatchOperation<T>
        {
            public IObservable source { get; set; }
            public IObservable<T> originatingSource { get; set; }
            public IReadOnlyList<T> operations { get; set; }

            public IOperation AllocateCopy()
            {
                var copy = source.context.AllocateOperation<BatchOperation>();

                copy.source = source;
                copy.originatingSource = originatingSource;
                copy.operations = operations.Select(x => (T)x.AllocateCopy()).ToArray();

                return copy;
            }

            public void Deallocate()
            {
                foreach (var op in operations)
                    op.Deallocate();

                var context = source.context;
                source = default;
                originatingSource = default;
                operations = default;
                context.DeallocateOperation(this);
            }
        }

        public bool immediate { get; } = false;
        public uint priority { get; private set; }
        public bool disposed { get; private set; }

        private IObservable<T> _source;
        private IObservableOperand<IBatchOperation<T>> _operand;
        private IDisposable _subscriptions;
        private bool _pending;

        private List<T> _batchedOperations = new List<T>();

        public BatchObservable(IObservable<T> source, IObservableOperand<IBatchOperation<T>> operand)
        {
            priority = source.context.AllocateObserverPriority();

            _source = source;
            _operand = operand;

            _subscriptions = source.Subscribe(
                onOperation: HandleSourceOperation,
                onError: operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        private void HandleSourceOperation(T operation)
        {
            _batchedOperations.Add((T)operation.AllocateCopy());

            if (_pending)
                return;

            _pending = true;
            _source.context.RegisterPendingObserver(this);
            _source.context.NotifyPendingObserversIfNecessary();
        }

        public IReadOnlyList<IBatchOperation<T>> GetInitializationOperations()
        {
            var operation = _source.context.AllocateOperation<BatchOperation>();

            operation.source = _operand.operationSource;
            operation.originatingSource = _source;
            operation.operations = _source.GetInitializationOperations();

            return new IBatchOperation<T>[] { operation };
        }

        public void SendNext()
        {
            _pending = false;

            var operation = _source.context.AllocateOperation<BatchOperation>();

            operation.source = _operand.operationSource;
            operation.originatingSource = _source;
            operation.operations = _batchedOperations.ToArray();

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