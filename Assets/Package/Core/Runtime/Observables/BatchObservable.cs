using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IBatchOperation<T> : IOperation<IReadOnlyList<T>> { }

    public class BatchObservable<T> : IInitializationOperationsProvider<IBatchOperation<T>>, IPendingObserver where T : IOperation
    {
        private class BatchOperation : IBatchOperation<T>
        {
            public IObservable<IOperation> source { get; set; }
            public IReadOnlyList<T> value { get; set; }

            private OperationPool<BatchOperation> _pool;

            public BatchOperation(OperationPool<BatchOperation> pool)
            {
                _pool = pool;
            }

            public IOperation Duplicate()
            {
                var duplicate = _pool.Allocate();
                duplicate.source = source;
                duplicate.value = value;
                return duplicate;
            }

            public void Dispose()
            {
                source = default;
                value = default;
                _pool.Deallocate(this);
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
        private OperationPool<BatchOperation> _operationPool;

        public BatchObservable(IObservable<T> source, IObservableOperand<IBatchOperation<T>> operand)
        {
            _operationPool = new OperationPool<BatchOperation>(pool => new BatchOperation(pool));
            priority = source.context.AllocateObserverPriority();

            _source = source;
            _operand = operand;

            _subscriptions = source.Subscribe(new Observer<T>(
                onNext: HandleSourceOperation,
                onError: operand.OnError,
                onDispose: Dispose,
                immediate: true
            ));
        }

        private BatchOperation AllocateOperation(IReadOnlyList<T> value)
        {
            var operation = _operationPool.Allocate();

            operation.source = (IObservable<IOperation>)_source;
            operation.value = value;

            return operation;
        }

        private void HandleSourceOperation(T operation)
        {
            _batchedOperations.Add((T)operation.Duplicate());

            if (_pending)
                return;

            _pending = true;
            _source.context.RegisterPendingObserver(this);
            _source.context.NotifyPendingObserversIfNecessary();
        }

        public IReadOnlyList<IBatchOperation<T>> GetInitializationOperations()
            => new IBatchOperation<T>[] { AllocateOperation(_source.GetInitializationOperations().Select(x => (T)x.Duplicate()).ToArray()) };

        public void SendNext()
        {
            _pending = false;

            var batch = _batchedOperations.ToArray();
            _batchedOperations.Clear();

            _operand.EnqueuePendingOperation(AllocateOperation(batch));
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