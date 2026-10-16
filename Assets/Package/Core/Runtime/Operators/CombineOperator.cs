using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CombineOperator<T> : IInitializationOperationsProvider<T>
    {
        private ISetObservable<IEventStreamObservable<T>> _source;
        private ObservableEventStream<T> _operand;
        private bool _disposeOnSourceEmpty;
        private uint _elementPriority;
        private Dictionary<IEventStreamObservable<T>, IDisposable> _observables = new Dictionary<IEventStreamObservable<T>, IDisposable>();
        private IDisposable _subscriptions;
        private bool _disposed;

        public CombineOperator(ISetObservable<IEventStreamObservable<T>> source, ObservableEventStream<T> operand, bool disposeOnSourceEmpty = false)
        {
            _source = source;
            _operand = operand;
            _disposeOnSourceEmpty = disposeOnSourceEmpty;
            _elementPriority = _source.context.AllocateObserverPriority();
            _subscriptions = _source.Subscribe(new SetObserver<IEventStreamObservable<T>>(
                onAdd: HandleElementAdded,
                onRemove: HandleElementRemoved,
                onError: _operand.OnError,
                onDispose: Dispose
            ), immediate: true);
        }

        public IEnumerable<T> GetInitializationOperations()
        {
            List<T> initOps = new List<T>();

            foreach (var element in _observables.Keys)
            {
                var subscription = element.Subscribe(new EventStreamObserver<T>(x => initOps.Add(x)));
                subscription.Dispose();
            }

            foreach (var op in initOps)
                yield return op;
        }

        private void HandleElementAdded(uint _, IEventStreamObservable<T> observable)
        {
            _observables.Add(
                observable,
                observable.Subscribe(new EventStreamObserver<T>(
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

        private void HandleElementRemoved(uint _, IEventStreamObservable<T> observable)
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