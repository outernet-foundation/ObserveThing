using System;

namespace ObserveThing
{
    public class CastValueObservable<T> : ObservableValueBase<T>
    {
        private IValueObservable _source;
        private IDisposable _subscription;

        public CastValueObservable(IValueObservable source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscription = _source.Subscribe(
                onNext: x => SetValueInternal((T)x),
                onError: OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscription?.Dispose();
            _subscription = null;
            SetValueInternal(default);
        }

        protected override void DisposeInternal()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}