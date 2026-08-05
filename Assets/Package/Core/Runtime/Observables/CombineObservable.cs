using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ObserveThing
{
    public class CombineObservable : IInitializationOperationsProvider
    {
        private ISetObservable<IObservable> _source;
        private IOperationObservableOperand _operand;
        private bool _disposeOnSourceEmpty;
        private uint _elementPriority;
        private Dictionary<IObservable, IDisposable> _observables = new Dictionary<IObservable, IDisposable>();
        private Stack<Operation> _operationPool = new Stack<Operation>();
        private IDisposable _subscriptions;

        public CombineObservable(ISetObservable<IObservable> source, IOperationObservableOperand operand, bool disposeOnSourceEmpty = false)
        {
            _source = source;
            _operand = operand;
            _disposeOnSourceEmpty = disposeOnSourceEmpty;
            _elementPriority = _source.context.AllocateObserverPriority();
            _subscriptions = _source.Subscribe(new SetObserver<IObservable>(
                onAdd: HandleElementAdded,
                onRemove: HandleElementRemoved,
                onError: _operand.OnError,
                onDispose: Dispose
            ), immediate: true);
        }

        public IReadOnlyList<IOperation> GetInitializationOperations()
        {
            List<Operation> initOps = new List<Operation>();

            foreach (var element in _observables.Keys)
            {
                var subscription = element.Subscribe(new OperationObserver(x => new Operation(_source) { args = x }));
                subscription.Dispose();
            }

            return initOps;
        }

        private void HandleElementAdded(uint _, IObservable observable)
        {
            _observables.Add(
                observable,
                observable.Subscribe(new OperationObserver(
                    onNext: HandleElementChanged,
                    onError: _operand.OnError,
                    onDispose: () => HandleElementRemoved(0, observable)
                ), immediate: true, priority: _elementPriority)
            );
        }

        private void HandleElementRemoved(uint _, IObservable observable)
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
            var copy = _operationPool.TryPop(out var op) ? op : new Operation(operation.source);
            copy.args = operation.args;
            _operand.EnqueuePendingOperation(copy);
        }

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