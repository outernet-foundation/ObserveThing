using System;

namespace ObserveThing
{
    public class CastListObservable<T> : IDisposable
    {
        private IDisposable _subscription;
        private IListObserver<T> _receiver;
        private bool _disposed;

        public CastListObservable(IListObservable source, IListObserver<T> receiver)
        {
            _receiver = receiver;
            _subscription = source.SubscribeWithId(
                onAdd: (id, index, x) => _receiver.OnAdd(id, index, (T)x),
                onRemove: (id, index, x) => _receiver.OnRemove(id, index, (T)x),
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