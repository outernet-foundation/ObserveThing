using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class DistinctDynamic<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private ICollectionObserver<T> _receiver;
        private Dictionary<T, (uint id, int count)> _dataByElement = new Dictionary<T, (uint id, int count)>();
        private CollectionIdProvider _idProvider;
        private bool _disposed;

        public DistinctDynamic(ICollectionObservable<T> source, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;
            _idProvider = new CollectionIdProvider(x => _dataByElement.Values.Any(y => y.id == x));
            _sourceStream = source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(uint id, T value)
        {
            if (!_dataByElement.TryGetValue(value, out var data))
                data = new(_idProvider.GetUnusedId(), 0);

            _dataByElement[value] = new(data.id, data.count + 1);

            if (data.count == 0) // data here is the old version before incrementing
                _receiver.OnAdd(data.id, value);
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
                _receiver.OnRemove(data.id, value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _receiver.OnDispose();
        }
    }
}