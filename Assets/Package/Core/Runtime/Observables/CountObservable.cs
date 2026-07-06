using System;

namespace ObserveThing
{
    public class CountObservable<T> : ObservableValueBase<int>
    {
        private ICollectionObservable<T> _source;
        private IDisposable _subscriptions;

        public CountObservable(ICollectionObservable<T> source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = _source.Subscribe(
                onAdd: _ => SetValueInternal(_value + 1),
                onRemove: _ => SetValueInternal(_value - 1),
                onError: OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            SetValueInternal(default);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}
