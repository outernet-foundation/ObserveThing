using System;

namespace ObserveThing
{
    public class CastListObservable<T> : ObservableListBase<T>
    {
        private IListObservable _source;
        private IDisposable _subscription;

        public CastListObservable(IListObservable source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscription = _source.Subscribe(
                onAdd: (index, element) => InsertInternal(index, (T)element),
                onRemove: (index, element) => RemoveAtInternal(index),
                onError: OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscription?.Dispose();
            _subscription = null;
            ClearInternal();
        }

        protected override void DisposeInternal()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}