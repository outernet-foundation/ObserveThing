using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ShareDictionaryObservable<TKey, TValue> : IDictionaryOperator<TKey, TValue>
    {
        private IDictionaryOperator<TKey, TValue> _source;
        private IDisposable _sourceStream;
        private DictionaryObservable<TKey, TValue> _shared;
        private int _observerCount;
        private bool _disposed;

        public ShareDictionaryObservable(IDictionaryOperator<TKey, TValue> source, ObservationContext context = default)
        {
            _source = source;
            _shared = new DictionaryObservable<TKey, TValue>(context);
        }

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer)
        {
            _observerCount++;

            if (_observerCount == 1)
            {
                _sourceStream = _source.Subscribe(
                    immediate: true,
                    onAdd: kvp => _shared.Add(kvp.Key, kvp.Value),
                    onRemove: kvp => _shared.Remove(kvp.Key)
                );
            }

            return _shared.SubscribeWithId(
                immediate: observer.immediate,
                onAdd: observer.OnAdd,
                onRemove: observer.OnRemove,
                onError: observer.OnError,
                onDispose: () =>
                {
                    observer.OnDispose();

                    _observerCount--;
                    if (_observerCount == 0)
                    {
                        _sourceStream.Dispose();
                        _sourceStream = null;
                        _shared.Clear();
                    }
                }
            );
        }

        public IDisposable Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, value) => observer.OnAdd(id, value),
                onRemove: (id, value) => observer.OnRemove(id, value),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream?.Dispose();
            _shared.Dispose();
        }
    }
}