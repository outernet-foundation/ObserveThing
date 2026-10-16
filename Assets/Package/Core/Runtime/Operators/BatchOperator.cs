using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class BatchOperator<T> : IInitializationOperationsProvider<IReadOnlyList<T>>, IPendingObserver
    {
        public bool immediate { get; } = false;
        public uint priority { get; private set; }
        public bool disposed { get; private set; }

        private IEventStreamObservable<T> _source;
        private ObservableEventStream<IReadOnlyList<T>> _operand;
        private IDisposable _subscriptions;
        private bool _pending;

        private List<T> _batchedOperations = new List<T>();

        public BatchOperator(IEventStreamObservable<T> source, ObservableEventStream<IReadOnlyList<T>> operand)
        {
            priority = source.context.AllocateObserverPriority();

            _source = source;
            _operand = operand;

            _subscriptions = source.Subscribe(new EventStreamObserver<T>(
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

        public IEnumerable<IReadOnlyList<T>> GetInitializationOperations()
        {
            List<T> initOps = new List<T>();
            var subscription = _source.Subscribe(new EventStreamObserver<T>(x => initOps.Add(x)));
            subscription.Dispose();
            yield return initOps;
        }

        public void SendNext()
        {
            _pending = false;

            var batch = _batchedOperations.ToArray();
            _batchedOperations.Clear();

            _operand.EnqueueOperation(batch);
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