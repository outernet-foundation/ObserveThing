using System;

namespace ObserveThing
{
    public class ShareCollectionObservable<T> : ICollectionObservable<T>
    {
        private ICollectionObservable<T> _source;
        private IDisposable _sourceStream;
        private ListObservable<T> _shared;
        private int _observerCount;
        private bool _disposed;

        public ShareCollectionObservable(ICollectionObservable<T> source)
        {
            _source = source;
            _shared = new ListObservable<T>(new ObservationContext());
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
        {
            _observerCount++;

            if (_observerCount == 1)
            {
                _sourceStream = _source.Subscribe(
                    immediate: true,
                    onAdd: item => _shared.Add(item),
                    onRemove: item => _shared.Remove(item)
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