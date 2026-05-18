using System;

namespace ObserveThing
{
    public class CastValueObservable<T> : IDisposable
    {
        private IDisposable _subscription;
        private IValueObserver<T> _receiver;
        private bool _disposed;

        public CastValueObservable(IValueObservable source, IValueObserver<T> receiver)
        {
            _receiver = receiver;
            _subscription = source.Subscribe(
                onNext: x => _receiver.OnNext((T)x),
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _subscription.Dispose();
            _receiver.OnDispose();
        }
    }
}