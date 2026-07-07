using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class WhereObservable<T> : ObservableCollectionBase<T>
    {
        private ICollectionObservable<T> _source;
        private Func<T, IValueObservable<bool>> _where;
        private Dictionary<uint, EntryData> _dataById = new Dictionary<uint, EntryData>();
        private IDisposable _subscriptions;
        private bool _active;

        private class EntryData
        {
            public bool initialized;
            public T value;
            public bool included;
            public IDisposable subscription;
        }

        public WhereObservable(ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where) : base(source.context)
        {
            _source = source;
            _where = where;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
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

            foreach (var data in _dataById.Values)
                data.subscription.Dispose();

            _dataById.Clear();

            ClearInternal();
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        private void HandleAdd(uint id, T value)
        {
            var data = new EntryData() { value = value };
            _dataById.Add(id, data);
            data.subscription = _where(value).Subscribe(
                onNext: included =>
                {
                    data.included = included;

                    if (!data.initialized)
                    {
                        data.initialized = true;

                        if (!included)
                            return;
                    }

                    if (included)
                    {
                        AddInternal(id, data.value);
                    }
                    else if (data.initialized)
                    {
                        RemoveInternal(id);
                    }
                },
                onError: OnError,
                immediate: true
            );
        }

        private void HandleRemove(uint id, T value)
        {
            var data = _dataById[id];
            data.subscription.Dispose();
            _dataById.Remove(id);

            if (data.included)
                RemoveInternal(id);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            foreach (var data in _dataById.Values)
                data.subscription.Dispose();
        }
    }
}