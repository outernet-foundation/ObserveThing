using System;

namespace ObserveThing
{
    public class CastListObservable<T> : ObservableListBase<T>
    {
        private IListObservable _source;
        private IDisposable _subscription;
        private bool _active;

        public CastListObservable(IListObservable source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscription = _source.Subscribe(
                onAdd: (index, element) => InsertInternal(index, (T)element),
                onRemove: (index, element) => RemoveAtInternal(index),
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