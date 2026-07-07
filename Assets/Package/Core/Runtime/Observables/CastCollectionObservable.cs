using System;

namespace ObserveThing
{
    public class CastCollectionObservable<T> : ObservableCollectionBase<T>
    {
        private ICollectionObservable _source;
        private IDisposable _subscription;
        private bool _active;

        public CastCollectionObservable(ICollectionObservable source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscription = _source.SubscribeWithId(
                onAdd: (id, x) => AddInternal(id, (T)x),
                onRemove: (id, x) => RemoveInternal(id),
                onError: OnError,
                onDispose: HandleSourceDisposed,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            _subscription?.Dispose();
            _subscription = null;
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
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}