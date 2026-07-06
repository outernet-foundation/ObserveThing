using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class BatchObservable<T> : Observable<IReadOnlyList<T>>, IPendingObserver
    {
        public bool immediate { get; } = true;
        public uint priority { get; private set; }

        private IObservable<T> _source;
        private IDisposable _subscription;
        private bool _initialized;
        private bool _pending;

        private List<T> _batchedOperations = new List<T>();

        public BatchObservable(IObservable<T> source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscription = _source.Subscribe(new Observer<T>(
                onNext: HandleSourceOperation,
                onError: OnError,
                onDispose: Dispose,
                immediate: true
            ));

            priority = _source.context.AllocateObserverPriority();
            _initialized = true;
        }

        protected override void OnLastObserverRemoved()
        {
            context.DeallocateObserverPriority(priority);
            _subscription.Dispose();
            _initialized = false;
        }

        private void HandleSourceOperation(T operation)
        {
            _batchedOperations.Add(operation);

            if (_pending || !_initialized)
                return;

            _pending = true;
            context.RegisterPendingObserver(this);
        }

        public void SendNext()
        {
            _pending = false;

            var ops = _batchedOperations;
            _batchedOperations = new List<T>();
            EnqueuePendingOperation(ops);
        }

        protected override void DisposeInternal()
        {
            context.DeallocateObserverPriority(priority);
            _subscription.Dispose();
        }

        protected override IReadOnlyList<IReadOnlyList<T>> GetInitializationOperations()
        {
            var ops = _batchedOperations;
            _batchedOperations = new List<T>();
            return new IReadOnlyList<T>[] { ops };
        }
    }
}