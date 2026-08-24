using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class SelectManyObservable<T, U> : IDisposable
    {
        private class ElementData
        {
            public IDisposable subscription;
            public Dictionary<uint, uint> elementIds = new Dictionary<uint, uint>();
        }

        private ObservableCollection<U> _operand;
        private Func<T, ICollectionObservable<U>> _select;
        private Dictionary<uint, ElementData> _dataById = new Dictionary<uint, ElementData>();
        private CollectionIdProvider _idProvider;
        private IDisposable _subscriptions;

        public SelectManyObservable(ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> select, ObservableCollection<U> operand)
        {
            _select = select;
            _operand = operand;
            _idProvider = new CollectionIdProvider(x => _dataById.Values.Any(y => y.elementIds.ContainsKey(x)));
            _subscriptions = source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
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
                    _operand.Add(newId, subElement);
                },
                onRemove: (subId, subElement) =>
                {
                    var elementId = data.elementIds[subId];
                    data.elementIds.Remove(subId);
                    _operand.Remove(elementId);
                },
                onError: _operand.OnError,
                immediate: true
            );
        }

        private void HandleRemove(uint id, T element)
        {
            var data = _dataById[id];
            _dataById.Remove(id);
            data.subscription.Dispose();

            foreach (var elementId in data.elementIds.Values)
                _operand.Remove(elementId);
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            foreach (var data in _dataById.Values)
                data.subscription.Dispose();

            _operand.Dispose();
        }
    }
}