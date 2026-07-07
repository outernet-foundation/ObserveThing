using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ShallowCopyCollectionObservable<T> : ObservableCollectionBase<T>
    {
        private ICollectionObservable<IValueObservable<T>> _source;
        private Dictionary<uint, EntryData> _dataById = new Dictionary<uint, EntryData>();
        private IDisposable _subscriptions;
        private bool _active;

        private class EntryData
        {
            public IDisposable subscription;
            public bool initialized;
        }

        public ShallowCopyCollectionObservable(ICollectionObservable<IValueObservable<T>> source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: OnError,
                onDispose: HandleSourceDisposed,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            foreach (var data in _dataById.Values)
                data.subscription.Dispose();

            _dataById.Clear();

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

        private void HandleAdd(uint id, IValueObservable<T> observable)
        {
            var data = new EntryData();
            _dataById.Add(id, data);
            data.subscription = observable.Subscribe(
                onNext: x =>
                {
                    if (data.initialized)
                        RemoveInternal(id);

                    data.initialized = true;
                    AddInternal(id, x);
                },
                onError: OnError,
                immediate: true
            );
        }

        private void HandleRemove(uint id, IValueObservable<T> observable)
        {
            var data = _dataById[id];
            _dataById.Remove(id);
            data.subscription.Dispose();
            RemoveInternal(id);
        }

        protected override void DisposeInternal()
        {
            foreach (var data in _dataById.Values)
                data.subscription.Dispose();

            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}