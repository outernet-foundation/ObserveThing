using System;

namespace ObserveThing
{
    public class ShallowCopyValueObservable<T> : ObservableValueBase<T>
    {
        private IValueObservable<IValueObservable<T>> _source;
        private Observer<IValueOperation<T>> _nestedObserver;
        private IDisposable _nestedSubscription;
        private IDisposable _subscriptions;
        private bool _active;

        public ShallowCopyValueObservable(IValueObservable<IValueObservable<T>> source) : base(source.context)
        {
            _source = source;
            _nestedObserver = new Observer<IValueOperation<T>>(
                onNext: x => SetValueInternal(x.value),
                onError: OnError,
                immediate: true
            );
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.Subscribe(
                onNext: HandleNext,
                onDispose: HandleSourceDisposed,
                onError: OnError,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            _subscriptions?.Dispose();
            _subscriptions = null;

            _nestedSubscription?.Dispose();
            _nestedSubscription = null;

            SetValueInternal(default);
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        private void HandleNext(IValueObservable<T> value)
        {
            _nestedSubscription?.Dispose();
            _nestedSubscription = null;

            if (value == null)
            {
                SetValueInternal(default);
                return;
            }

            _nestedSubscription = value.Subscribe(_nestedObserver);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            _nestedSubscription?.Dispose();
            _nestedSubscription = null;
        }
    }
}