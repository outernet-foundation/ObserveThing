using System;

namespace ObserveThing
{
    public class CountObservable<T> : ObservableValueBase<int>
    {
        private ICollectionObservable<T> _source;
        private IDisposable _subscriptions;
        private bool _active;

        public CountObservable(ICollectionObservable<T> source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.Subscribe(
                onAdd: _ => SetValueInternal(_value + 1),
                onRemove: _ => SetValueInternal(_value - 1),
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
            SetValueInternal(default);
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
