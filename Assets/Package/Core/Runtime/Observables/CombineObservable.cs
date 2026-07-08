using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class CombineObservable : IInitializationOperationsProvider<IOperation>
    {
        private ISetObservable<IObservable> _source;
        private IObservableOperand<IOperation> _operand;
        private bool _disposeOnSourceEmpty;
        private uint _elementPriority;
        private Dictionary<IObservable, IDisposable> _observables = new Dictionary<IObservable, IDisposable>();
        private IDisposable _subscriptions;

        public CombineObservable(ISetObservable<IObservable> source, IObservableOperand<IOperation> operand, bool disposeOnSourceEmpty = false)
        {
            _source = source;
            _operand = operand;
            _disposeOnSourceEmpty = disposeOnSourceEmpty;
            _elementPriority = _source.context.AllocateObserverPriority();
            _subscriptions = _source.Subscribe(
                onAdd: HandleElementAdded,
                onRemove: HandleElementRemoved,
                onError: _operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        public IReadOnlyList<IOperation> GetInitializationOperations()
            => _observables.Keys.SelectMany(x => x.GetInitializationOperations()).ToArray();

        private void HandleElementAdded(IObservable observable)
        {
            _observables.Add(
                observable,
                observable.Subscribe(new Observer(
                    onNext: HandleElementChanged,
                    onError: _operand.OnError,
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
            => _operand.EnqueuePendingOperation(operation.AllocateCopy());

        public void Dispose()
        {
            _source.context.DeallocateObserverPriority(_elementPriority);

            foreach (var subscription in _observables.Values)
                subscription.Dispose();

            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }
}