using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public struct CollectionOpData<T>
    {
        public T value;
        public uint id;
        public bool isRemove;
    }

    public class ShareCollectionObservable<T> : ICollectionObservable<T>
    {
        private ICollectionObservable<T> _source;
        private IDisposable _sourceStream;
        private Dictionary<uint, T> _collection = new Dictionary<uint, T>();
        private SynchronizedNotificationQueue<ICollectionObserver<T>, CollectionOpData<T>> _notificationQueue;
        private int _observerCount;
        private bool _disposed;

        public ShareCollectionObservable(ICollectionObservable<T> source, SynchronizationContext context = default)
        {
            _source = source;
            _notificationQueue = new SynchronizedNotificationQueue<ICollectionObserver<T>, CollectionOpData<T>>(NotifyObserver, context);
        }

        private void NotifyObserver(ICollectionObserver<T> observer, CollectionOpData<T> notification)
        {
            if (notification.isRemove)
            {
                observer.OnRemove(notification.id, notification.value);
            }
            else
            {
                observer.OnAdd(notification.id, notification.value);
            }
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
        {
            _observerCount++;

            if (_observerCount == 1)
            {
                _sourceStream = _source.SubscribeWithId(
                    immediate: true,
                    onAdd: (id, item) =>
                    {
                        _collection.Add(id, item);
                        _notificationQueue.EnqueueNotify(new() { id = id, value = item, isRemove = false });
                    },
                    onRemove: (id, item) =>
                    {
                        _collection.Remove(id);
                        _notificationQueue.EnqueueNotify(new() { id = id, value = item, isRemove = true });
                    }
                );
            }

            foreach (var kvp in _collection)
                observer.OnAdd(kvp.Key, kvp.Value);

            return _notificationQueue.AddObserver(new CollectionObserver<T>(
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
                    }
                }
            ));
        }

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new CollectionObserver<T>(
                onAdd: (_, _) => observer.OnChange(),
                onRemove: (_, _) => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream?.Dispose();
            _notificationQueue.Dispose();
        }
    }
}