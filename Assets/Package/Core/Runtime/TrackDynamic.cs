using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObserveThing
{
    public class TrackDynamic<TKey, TValue> : IDisposable
    {
        private IDisposable _sourceStream;
        private IDisposable _keyStream;
        private IValueObserver<(bool present, TValue value)> _receiver;

        private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        private TKey _key = default;
        private bool _present = false;

        private bool _disposed;

        public TrackDynamic(IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key, IValueObserver<(bool present, TValue value)> receiver)
        {
            _receiver = receiver;
            _sourceStream = source.Subscribe(
                onAdd: kvp =>
                {
                    _dict.Add(kvp.Key, kvp.Value);
                    if (Equals(kvp.Key, key))
                    {
                        _present = true;
                        _receiver.OnNext(new(_present, kvp.Value));
                    }
                },
                onRemove: kvp =>
                {
                    if (_dict.Remove(kvp.Key) && Equals(kvp.Key, _key))
                    {
                        _present = false;
                        _receiver.OnNext(new(_present, default));
                    }
                },
                onError: _receiver.OnError
            );

            _keyStream = key.Subscribe(
                onNext: key =>
                {
                    _key = key;

                    if (_dict.TryGetValue(key, out var value))
                    {
                        _present = true;
                        _receiver.OnNext(new(_present, value));
                    }
                    else if (_present)
                    {
                        _present = false;
                        _receiver.OnNext(new(_present, default));
                    }
                },
                onError: _receiver.OnError
            );
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();
            _keyStream.Dispose();

            _receiver.OnDispose();
        }
    }
}
