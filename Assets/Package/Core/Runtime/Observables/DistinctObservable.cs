using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class DistinctObservable<T> : ObservableSetBase<T>
    {
        private ICollectionObservable<T> _source;
        private Dictionary<T, (uint id, int count)> _dataByElement = new Dictionary<T, (uint id, int count)>();
        private CollectionIdProvider _idProvider;
        private IDisposable _subscriptions;
        private bool _active;

        public DistinctObservable(ICollectionObservable<T> source) : base(source.context)
        {
            _source = source;
            _idProvider = new CollectionIdProvider(x => _dataByElement.Values.Any(y => y.id == x));
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
            _subscriptions?.Dispose();
            _subscriptions = null;
            _dataByElement.Clear();
            _idProvider.Reset();
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
            if (!_dataByElement.TryGetValue(value, out var data))
                data = new(_idProvider.GetUnusedId(), 0);

            _dataByElement[value] = new(data.id, data.count + 1);

            if (data.count == 0) // data here is the old version before incrementing
                AddInternal(value);
        }

        private void HandleRemove(uint id, T value)
        {
            var data = _dataByElement[value];

            if (data.count == 1)
            {
                _dataByElement.Remove(value);
            }
            else
            {
                _dataByElement[value] = new(data.id, data.count - 1);
            }

            if (data.count == 1)
                RemoveInternal(value);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}