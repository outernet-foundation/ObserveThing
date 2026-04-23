using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CombineObservable : ObservableOperator<IOperation>
    {
        private IDisposable _sourceSubscription;
        private Dictionary<IObservable, IDisposable> _subscriptions = new Dictionary<IObservable, IDisposable>();

        public CombineObservable(ISetObservable<IObservable> source, IObserver<IOperation> receiver) : this(default, source, receiver) { }
        public CombineObservable(ObservationContext context, ISetObservable<IObservable> source, IObserver<IOperation> receiver) : base(context, receiver)
        {
            _sourceSubscription = source.Subscribe(
                onAdd: HandleSourceAdded,
                onRemove: HandleSourceRemoved,
                onError: receiver.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        private void HandleSourceAdded(IObservable observable)
        {
            _subscriptions.Add(
                observable,
                observable.Subscribe(
                    onOperation: HandleSourceChanged,
                    onDispose: () => HandleSourceRemoved(observable),
                    immediate: true
                )
            );
        }

        private void HandleSourceRemoved(IObservable observable)
        {
            if (!_subscriptions.TryGetValue(observable, out var subscription))
                return;

            subscription.Dispose();
            _subscriptions.Remove(observable);
        }

        private void HandleSourceChanged(IReadOnlyList<IOperation> ops)
        {
            if (ops == null)
                return;

            foreach (var op in ops)
                EnqueuePendingOperation(op.Clone());
        }

        protected override void DisposeInternal()
        {
            foreach (var subscription in _subscriptions.Values)
                subscription.Dispose();

            _subscriptions.Clear();
            _sourceSubscription?.Dispose();
        }
    }
}