using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class SelectManyObservable<T, U> : ObservableCollectionBase<U>
    {
        private class ElementData
        {
            public IDisposable subscription;
            public Dictionary<uint, uint> elementIds = new Dictionary<uint, uint>();
        }

        private ICollectionObservable<T> _source;
        private Func<T, ICollectionObservable<U>> _select;
        private Dictionary<uint, ElementData> _dataById = new Dictionary<uint, ElementData>();
        private CollectionIdProvider _idProvider;
        private IDisposable _subscriptions;
        private bool _active;

        public SelectManyObservable(ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> select) : base(source.context)
        {
            _source = source;
            _select = select;
            _idProvider = new CollectionIdProvider(x => _dataById.Values.Any(y => y.elementIds.ContainsKey(x)));
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

            foreach (var data in _dataById.Values)
                data.subscription.Dispose();

            _dataById.Clear();
            _idProvider.Reset();

            ClearInternal();
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        private void HandleAdd(uint id, T element)
        {
            var data = new ElementData();
            _dataById.Add(id, data);
            data.subscription = _select(element).SubscribeWithId(
                onAdd: (subId, subElement) =>
                {
                    var newId = _idProvider.GetUnusedId();
                    data.elementIds.Add(subId, newId);
                    AddInternal(newId, subElement);
                },
                onRemove: (subId, subElement) =>
                {
                    var elementId = data.elementIds[subId];
                    data.elementIds.Remove(subId);
                    RemoveInternal(elementId);
                },
                onError: OnError,
                immediate: true
            );
        }

        private void HandleRemove(uint id, T element)
        {
            var data = _dataById[id];
            _dataById.Remove(id);
            data.subscription.Dispose();

            foreach (var elementId in data.elementIds.Values)
                RemoveInternal(elementId);
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