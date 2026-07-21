using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ObserveThing
{
    public class CombineObservable<T> : IInitializationOperationsProvider<T> where T : IOperation
    {
        private IObservable<ISetOperation<IObservable<T>>> _source;
        private IObservableOperand<T> _operand;
        private bool _disposeOnSourceEmpty;
        private uint _elementPriority;
        private Dictionary<IObservable<T>, IDisposable> _observables = new Dictionary<IObservable<T>, IDisposable>();
        private IDisposable _subscriptions;

        public CombineObservable(IObservable<ISetOperation<IObservable<T>>> source, IObservableOperand<T> operand, bool disposeOnSourceEmpty = false)
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

        public IReadOnlyList<T> GetInitializationOperations()
            => _observables.Keys.SelectMany(x => x.GetInitializationOperations()).Select(x => (T)x.Duplicate()).ToArray();

        private void HandleElementAdded(IObservable<T> observable)
        {
            _observables.Add(
                observable,
                observable.Subscribe(new Observer<T>(
                    onNext: HandleElementChanged,
                    onError: _operand.OnError,
                    onDispose: () => HandleElementRemoved(observable),
                    overridePriority: _elementPriority,
                    immediate: true
                ))
            );
        }

        private void HandleElementRemoved(IObservable<T> observable)
        {
            if (!_observables.TryGetValue(observable, out var subscription))
                return;

            _observables.Remove(observable);
            subscription.Dispose();

            if (_disposeOnSourceEmpty && _observables.Count == 0)
                Dispose();
        }

        protected void HandleElementChanged(T operation)
            => _operand.EnqueuePendingOperation((T)operation.Duplicate());

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