using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class SelectManyDynamic<T, U> : IDisposable
    {
        private class ElementData
        {
            public IDisposable subscription;
            public Dictionary<uint, (uint id, U element)> elements = new Dictionary<uint, (uint id, U element)>();
        }

        private IDisposable _sourceStream;
        private Func<T, ICollectionObservable<U>> _select;
        private ICollectionObserver<U> _receiver;
        private Dictionary<uint, ElementData> _dataById = new Dictionary<uint, ElementData>();
        private CollectionIdProvider _idProvider;
        private bool _disposed;

        public SelectManyDynamic(ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> select, ICollectionObserver<U> receiver)
        {
            _receiver = receiver;
            _select = select;
            _idProvider = new CollectionIdProvider(x => _dataById.Values.Any(y => y.elements.ContainsKey(x)));
            _sourceStream = source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: receiver.OnError,
                onDispose: Dispose
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
                    data.elements.Add(subId, (newId, subElement));
                    _receiver.OnAdd(newId, subElement);
                },
                onRemove: (subId, subElement) =>
                {
                    var subData = data.elements[subId];
                    data.elements.Remove(subId);
                    _receiver.OnRemove(subData.id, subData.element);
                },
                onError: _receiver.OnError
            );
        }

        private void HandleRemove(uint id, T element)
        {
            var data = _dataById[id];
            _dataById.Remove(id);
            data.subscription.Dispose();
            foreach (var subElement in data.elements.Values)
                _receiver.OnRemove(subElement.id, subElement.element);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            foreach (var data in _dataById.Values)
                data.subscription.Dispose();

            _receiver.OnDispose();
        }
    }
}