using System;

namespace ObserveThing
{
    public class SelectValueObservable<T, U> : ObservableValueBase<U>
    {
        private IValueObservable<T> _source;
        private IDisposable _subscriptions;
        private Func<T, U> _select;
        private bool _active;

        public SelectValueObservable(IValueObservable<T> source, Func<T, U> select) : base(source.context)
        {
            _source = source;
            _select = select;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.Subscribe(
                onNext: x => SetValueInternal(_select(x)),
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