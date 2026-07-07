using System;

namespace ObserveThing
{
    public class SelectListObservable<T, U> : ObservableListBase<U>
    {
        private IListObservable<T> _source;
        private Func<T, U> _select;
        private IDisposable _subscriptions;
        private bool _active;

        public SelectListObservable(IListObservable<T> source, Func<T, U> select) : base(source.context)
        {
            _source = source;
            _select = select;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.Subscribe(
                onAdd: (index, value) => InsertInternal(index, _select(value)),
                onRemove: (index, _) => RemoveAtInternal(index),
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