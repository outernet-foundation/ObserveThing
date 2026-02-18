using System;
using UnityEngine;

namespace ObserveThing
{
    public class CountDynamic<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private int _count;
        private IValueObserver<int> _receiver;
        private bool _disposed;

        public CountDynamic(ICollectionObservable<T> source, IValueObserver<int> receiver)
        {
            _sourceStream = source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(T _)
        {
            _count++;
            _receiver.OnNext(_count);
        }

        private void HandleRemove(T _)
        {
            _count--;
            _receiver.OnNext(_count);
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
