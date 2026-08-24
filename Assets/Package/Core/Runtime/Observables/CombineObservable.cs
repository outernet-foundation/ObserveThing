using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CombineObservable<T> : IInitializationOperationsProvider<T> where T : IOperation
    {
        private ISetObservable<IObservable<T>> _source;
        private Observable<T> _operand;
        private bool _disposeOnSourceEmpty;
        private uint _elementPriority;
        private Dictionary<IObservable<T>, IDisposable> _observables = new Dictionary<IObservable<T>, IDisposable>();
        private IDisposable _subscriptions;
        private bool _disposed;

        public CombineObservable(ISetObservable<IObservable<T>> source, Observable<T> operand, bool disposeOnSourceEmpty = false)
        {
            _source = source;
            _operand = operand;
            _disposeOnSourceEmpty = disposeOnSourceEmpty;
            _elementPriority = _source.context.AllocateObserverPriority();
            _subscriptions = _source.Subscribe(new SetObserver<IObservable<T>>(
                onAdd: HandleElementAdded,
                onRemove: HandleElementRemoved,
                onError: _operand.OnError,
                onDispose: Dispose
            ), immediate: true);
        }

        public IReadOnlyList<T> GetInitializationOperations()
        {
            List<T> initOps = new List<T>();

            foreach (var element in _observables.Keys)
            {
                var subscription = element.Subscribe(new Observer<T>(x => initOps.Add(x)));
                subscription.Dispose();
            }

            return initOps;
        }

        private void HandleElementAdded(uint _, IObservable<T> observable)
        {
            _observables.Add(
                observable,
                observable.Subscribe(new Observer<T>(
                    onNext: HandleElementChanged,
                    onError: _operand.OnError,
                    onDispose: () =>
                    {
                        if (_disposed)
                            return;

                        HandleElementRemoved(0, observable);
                    }
                ), immediate: true, priority: _elementPriority)
            );
        }

        private void HandleElementRemoved(uint _, IObservable<T> observable)
        {
            if (!_observables.TryGetValue(observable, out var subscription))
                return;

            _observables.Remove(observable);
            subscription.Dispose();

            if (_disposeOnSourceEmpty && _observables.Count == 0)
                Dispose();
        }

        protected void HandleElementChanged(T operation)
        {
            _operand.EnqueueOperation(operation);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _source.context.DeallocateObserverPriority(_elementPriority);

            foreach (var subscription in _observables.Values)
                subscription.Dispose();

            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }
}