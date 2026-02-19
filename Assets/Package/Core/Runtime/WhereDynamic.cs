using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class WhereDynamic<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private ICollectionObserver<T> _receiver;
        private Func<T, IValueObservable<bool>> _where;
        private Dictionary<uint, IDisposable> _subscriptions = new Dictionary<uint, IDisposable>();
        public CollectionIdProvider _idProvider;
        private bool _disposed;

        public WhereDynamic(ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;
            _where = where;
            _idProvider = new CollectionIdProvider(x => _subscriptions.ContainsKey(x));
            _sourceStream = source.Subscribe(
                HandleAdd,
                HandleRemove,
                _receiver.OnError,
                Dispose
            );
        }

        private void HandleAdd(uint id, T value)
        {
            _subscriptions.Add(id, _where(value).Subscribe(
                onNext: included =>
                {
                    if (included)
                    {
                        _receiver.OnAdd(id, value);
                    }
                    else
                    {
                        _receiver.OnRemove(id, value);
                    }
                },
                onError: _receiver.OnError
            ));
        }

        private void HandleRemove(uint id, T value)
        {
            _subscriptions[id].Dispose();
            _subscriptions.Remove(id);
            _receiver.OnRemove(id, value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            foreach (var subscription in _subscriptions.Values)
                subscription.Dispose();

            _receiver.OnDispose();
        }
    }
}