using System;

namespace ObserveThing
{
    public class CastCollectionObservable<T> : IDisposable
    {
        private IDisposable _subscription;
        private ICollectionObserver<T> _receiver;
        private bool _disposed;

        public CastCollectionObservable(ICollectionObservable source, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;
            _subscription = source.SubscribeWithId(
                onAdd: (id, x) => _receiver.OnAdd(id, (T)x),
                onRemove: (id, x) => _receiver.OnRemove(id, (T)x),
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