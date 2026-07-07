using System;

namespace ObserveThing
{
    public class SkipWhileObservable<T> : ObservableValueBase<T>
    {
        private IValueObservable<T> _source;
        private Func<bool> _skipWhile;
        private IDisposable _subscriptions;
        private bool _active;

        public SkipWhileObservable(IValueObservable<T> source, Func<bool> skipWhile) : base(source.context)
        {
            _source = source;
            _skipWhile = skipWhile;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.Subscribe(
                onNext: HandleSourceChanged,
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

        private void HandleSourceChanged(T value)
        {
            if (!_skipWhile())
                SetValueInternal(value);
        }
    }
}