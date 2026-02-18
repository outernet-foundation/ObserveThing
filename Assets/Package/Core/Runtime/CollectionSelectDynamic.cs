using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CollectionSelectDynamic<T, U> : IDisposable
    {
        private class ElementData
        {
            public int count;
            public U latest;
        }

        private IDisposable _sourceStream;
        private Func<T, U> _select;
        private ICollectionObserver<U> _receiver;
        private Dictionary<T, ElementData> _dataByElement = new Dictionary<T, ElementData>();
        private bool _disposed;

        public CollectionSelectDynamic(ICollectionObservable<T> source, Func<T, U> select, ICollectionObserver<U> receiver)
        {
            _receiver = receiver;
            _select = select;
            _sourceStream = source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(T value)
        {
            if (!_dataByElement.TryGetValue(value, out var data))
            {
                data = new ElementData() { latest = _select(value) };
                _dataByElement.Add(value, data);
            }

            data.count++;
            _receiver.OnAdd(data.latest);
        }

        private void HandleRemove(T value)
        {
            var data = _dataByElement[value];
            data.count--;

            if (data.count == 0)
                _dataByElement.Remove(value);

            _receiver.OnRemove(data.latest);
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