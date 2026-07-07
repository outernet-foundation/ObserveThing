using System;

namespace ObserveThing
{
    public class WithPreviousObservable<T> : ObservableValueBase<(T current, T previous)>
    {
        private IValueObservable<T> _source;
        private T _previousValue;
        private IDisposable _subscriptions;
        private bool _active;

        public WithPreviousObservable(IValueObservable<T> source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.Subscribe(
                onNext: HandleNext,
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
            _previousValue = default;
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        private void HandleNext(T value)
        {
            var pair = (value, _previousValue);
            _previousValue = value;
            SetValueInternal(pair);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}
