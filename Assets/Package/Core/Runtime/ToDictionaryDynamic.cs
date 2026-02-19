using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ToDictionaryDynamic<TSource, TKey, TValue> : IDisposable
    {
        private IDisposable _sourceStream;
        private Func<TSource, IValueObservable<TKey>> _selectKey;
        private Func<TSource, IValueObservable<TValue>> _selectValue;
        private IDictionaryObserver<TKey, TValue> _receiver;
        private Dictionary<uint, EntryData> _dataById = new Dictionary<uint, EntryData>();
        private bool _disposed;

        private class EntryData
        {
            public TKey key;
            public TValue value;
            public IDisposable keyStream;
            public IDisposable valueStream;
            public bool initialized;
        }

        public ToDictionaryDynamic(ICollectionObservable<TSource> source, Func<TSource, IValueObservable<TKey>> selectKey, Func<TSource, IValueObservable<TValue>> selectValue, IDictionaryObserver<TKey, TValue> receiver)
        {
            _receiver = receiver;
            _selectKey = selectKey;
            _selectValue = selectValue;
            _sourceStream = source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(uint id, TSource element)
        {
            var data = new EntryData();
            _dataById.Add(id, data);

            data.keyStream = _selectKey(element).Subscribe(
                onNext: key =>
                {
                    if (!data.initialized)
                    {
                        data.key = key;
                        data.initialized = true;
                        _receiver.OnAdd(id, new KeyValuePair<TKey, TValue>(data.key, data.value));
                        return;
                    }

                    _receiver.OnRemove(id, new KeyValuePair<TKey, TValue>(data.key, data.value));
                    data.key = key;
                    _receiver.OnAdd(id, new KeyValuePair<TKey, TValue>(data.key, data.value));
                },
                onError: _receiver.OnError
            );

            data.valueStream = _selectValue(element).Subscribe(
                onNext: value =>
                {
                    _receiver.OnRemove(id, new KeyValuePair<TKey, TValue>(data.key, data.value));
                    data.value = value;
                    _receiver.OnAdd(id, new KeyValuePair<TKey, TValue>(data.key, data.value));
                },
                onError: _receiver.OnError
            );
        }

        private void HandleRemove(uint id, TSource element)
        {
            var data = _dataById[id];
            _dataById.Remove(id);
            data.keyStream.Dispose();
            data.valueStream.Dispose();
            _receiver.OnRemove(id, new KeyValuePair<TKey, TValue>(data.key, data.value));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            foreach (var data in _dataById.Values)
            {
                data.keyStream.Dispose();
                data.valueStream.Dispose();
            }

            _receiver.OnDispose();
        }
    }
}
