using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class DistinctDynamic<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private ICollectionObserver<T> _receiver;
        private Dictionary<T, int> _countByElement = new Dictionary<T, int>();
        private bool _disposed;

        public DistinctDynamic(ICollectionObservable<T> source, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;
            _sourceStream = source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(T value)
        {
            if (!_countByElement.TryGetValue(value, out var count))
                count = 0;

            _countByElement[value] = count + 1;

            if (count == 0)
                _receiver.OnAdd(value);
        }

        private void HandleRemove(T value)
        {
            var count = _countByElement[value];

            if (count == 1)
            {
                _countByElement.Remove(value);
            }
            else
            {
                _countByElement[value] = count - 1;
            }

            if (count == 1)
                _receiver.OnRemove(value);
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