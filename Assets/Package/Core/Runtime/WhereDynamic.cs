using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ObserveThing
{
    public class WhereDynamic<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private ICollectionObserver<T> _receiver;
        private Func<T, IValueObservable<bool>> _where;
        private Dictionary<uint, EntryData> _dataById = new Dictionary<uint, EntryData>();
        public CollectionIdProvider _idProvider;
        private bool _disposed;

        private class EntryData
        {
            public bool initialized;
            public T value;
            public bool included;
            public IDisposable subscription;
        }

        public WhereDynamic(ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;
            _where = where;
            _idProvider = new CollectionIdProvider(x => _dataById.ContainsKey(x));
            _sourceStream = source.SubscribeWithId(
                HandleAdd,
                HandleRemove,
                _receiver.OnError,
                Dispose
            );
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
                        _receiver.OnAdd(id, data.value);
                    }
                    else if (data.initialized)
                    {
                        _receiver.OnRemove(id, data.value);
                    }
                },
                onError: _receiver.OnError
            );
        }

        private void HandleRemove(uint id, T value)
        {
            var data = _dataById[id];
            data.subscription.Dispose();
            _dataById.Remove(id);

            if (data.included)
                _receiver.OnRemove(id, value);
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