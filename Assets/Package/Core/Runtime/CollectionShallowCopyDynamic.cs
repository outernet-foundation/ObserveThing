using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CollectionShallowCopyDynamic<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private ICollectionObserver<T> _receiver;
        private Dictionary<uint, EntryData> _dataById = new Dictionary<uint, EntryData>();
        private bool _disposed;

        private class EntryData
        {
            public IDisposable subscription;
            public T latest;
            public bool initialized;
        }

        public CollectionShallowCopyDynamic(ICollectionObservable<IValueObservable<T>> source, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;
            _sourceStream = source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(uint id, IValueObservable<T> observable)
        {
            var data = new EntryData();
            _dataById.Add(id, data);
            data.subscription = observable.ObservableWithPrevious().Subscribe(
                onNext: x =>
                {
                    if (data.initialized)
                        _receiver.OnRemove(id, x.previous);

                    data.initialized = true;
                    data.latest = x.current;
                    _receiver.OnAdd(id, x.current);
                },
                onError: _receiver.OnError
            );
        }

        private void HandleRemove(uint id, IValueObservable<T> observable)
        {
            var data = _dataById[id];
            _dataById.Remove(id);
            data.subscription.Dispose();
            _receiver.OnRemove(id, data.latest);
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