using System;

namespace ObserveThing
{
    public class ShareSetObservable<T> : ISetOperator<T>
    {
        private ISetOperator<T> _source;
        private IDisposable _sourceStream;
        private SetObservable<T> _shared;
        private int _observerCount;
        private bool _disposed;

        public ShareSetObservable(ISetOperator<T> source, ObservationContext context = default)
        {
            _source = source;
            _shared = new SetObservable<T>(context);
        }

        public IDisposable Subscribe(ISetObserver<T> observer)
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

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new SetObserver<T>(
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