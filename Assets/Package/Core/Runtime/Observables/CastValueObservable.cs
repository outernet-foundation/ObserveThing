using System;

namespace ObserveThing
{
    public class CastValueObservable<T> : ObservableValueBase<T>
    {
        private IValueObservable _source;
        private IDisposable _subscription;
        private bool _active;

        public CastValueObservable(IValueObservable source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscription = _source.Subscribe(
                onNext: x => SetValueInternal((T)x),
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
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}