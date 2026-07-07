using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class OnEachObservable<T> : Observable<T> where T : IOperation
    {
        private IObservable<T> _source;
        private List<T> _initOperations = new List<T>();
        private IObserver<T> _then;
        private IDisposable _subscriptions;
        private bool _active;

        public OnEachObservable(IObservable<T> source, IObserver<T> then) : base(source.context)
        {
            _source = source;
            _then = then;
        }

        protected override IReadOnlyList<T> GetInitializationOperations()
            => _initOperations;

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.Subscribe(
                onOperation: op =>
                {
                    _then.OnNext(op);
                    EnqueuePendingOperation((T)op.Clone());
                },
                onError: exc =>
                {
                    _then.OnError(exc);
                    OnError(exc);
                },
                onDispose: HandleSourceDisposed
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            _then.OnDispose();
            Dispose();
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}