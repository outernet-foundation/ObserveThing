using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CombineObservable : Observable<IOperation>
    {
        private ISetObservable<IObservable> _source;
        private bool _disposeOnSourceEmpty;
        private IDisposable _subscriptions;
        private Dictionary<IObservable, IDisposable> _observables = new Dictionary<IObservable, IDisposable>();
        private List<IOperation> _initOperations = new List<IOperation>();
        private bool _active;
        private bool _initialized;
        private uint _elementPriority;

        public CombineObservable(ISetObservable<IObservable> source, bool disposeOnSourceEmpty = false) : base(source.context)
        {
            _source = source;
            _disposeOnSourceEmpty = disposeOnSourceEmpty;
        }

        protected override IReadOnlyList<IOperation> GetInitializationOperations()
            => _initOperations;

        protected override void OnFirstObserverAdded()
        {
            _active = true;

            _elementPriority = context.AllocateObserverPriority();
            _subscriptions = _source.Subscribe(
                onAdd: HandleElementAdded,
                onRemove: HandleElementRemoved,
                onError: OnError,
                onDispose: HandleSourceDisposed,
                immediate: true
            );

            _initialized = true;
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;

            context.DeallocateObserverPriority(_elementPriority);

            _subscriptions?.Dispose();
            _subscriptions = null;

            foreach (var subscription in _observables.Values)
                subscription.Dispose();

            _observables.Clear();
            _initialized = false;
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        private void HandleElementAdded(IObservable observable)
        {
            _observables.Add(
                observable,
                observable.Subscribe(new Observer(
                    onNext: HandleElementChanged,
                    onError: OnError,
                    onDispose: () => HandleElementRemoved(observable),
                    overridePriority: _elementPriority,
                    immediate: true
                ))
            );
        }

        private void HandleElementRemoved(IObservable observable)
        {
            if (!_observables.TryGetValue(observable, out var subscription))
                return;

            _observables.Remove(observable);
            subscription.Dispose();

            if (_disposeOnSourceEmpty && _observables.Count == 0)
                Dispose();
        }

        protected void HandleElementChanged(IOperation operation)
        {
            if (!_initialized)
            {
                _initOperations.Add(operation.Clone());
                return;
            }

            EnqueuePendingOperation(operation.Clone());
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            foreach (var subscription in _observables.Values)
                subscription.Dispose();
        }
    }
}