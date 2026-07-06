using System;

namespace ObserveThing
{
    public class CastCollectionObservable<T> : ObservableCollectionBase<T>
    {
        private ICollectionObservable _source;
        private IDisposable _subscription;

        public CastCollectionObservable(ICollectionObservable source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscription = _source.SubscribeWithId(
                onAdd: (id, x) => AddInternal(id, (T)x),
                onRemove: (id, x) => RemoveInternal(id),
                onError: OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscription?.Dispose();
            _subscription = null;
            ClearInternal();
        }

        protected override void DisposeInternal()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}