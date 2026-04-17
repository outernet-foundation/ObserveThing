using System;

namespace ObserveThing
{
    public class ShareListObservable<T> : IListOperator<T>
    {
        private IListOperator<T> _source;
        private IDisposable _sourceStream;
        private ListObservable<T> _shared;
        private int _observerCount;
        private bool _disposed;

        public ShareListObservable(IListOperator<T> source)
        {
            _source = source;
            _shared = new ListObservable<T>(new ObservationContext());
        }

        public IDisposable Subscribe(IListObserver<T> observer)
        {
            _observerCount++;

            if (_observerCount == 1)
            {
                _sourceStream = _source.Subscribe(
                    immediate: true,
                    onAdd: (index, item) => _shared.Insert(index, item),
                    onRemove: (index, item) => _shared.RemoveAt(index)
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
            => Subscribe(new ListObserver<T>(
                onAdd: (id, _, value) => observer.OnAdd(id, value),
                onRemove: (id, _, value) => observer.OnRemove(id, value),
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