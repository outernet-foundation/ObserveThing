using System;

namespace ObserveThing
{
    public class SelectCollectionObservable<T, U> : ObservableCollectionBase<U>
    {
        private ICollectionObservable<T> _source;
        private Func<T, U> _select;
        private IDisposable _subscriptions;
        private bool _active;

        public SelectCollectionObservable(ICollectionObservable<T> source, Func<T, U> select) : base(source.context)
        {
            _source = source;
            _select = select;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, value) => AddInternal(id, _select(value)),
                onRemove: (id, _) => RemoveInternal(id),
                onError: OnError,
                onDispose: HandleSourceDisposed,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            _subscriptions?.Dispose();
            _subscriptions = null;
            ClearInternal();
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}