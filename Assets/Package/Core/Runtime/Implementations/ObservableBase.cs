using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public abstract class ObservableBase<TObserver, TNotification> : IDisposable where TObserver : IObserverBase
    {
        private SynchronizationContext _context;
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _immediateObservers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _disposed;

        private int _immediateNotificationIndex = 0;
        private Queue<TNotification> _pendingNotifications = new Queue<TNotification>();

        private class ObserverData : IDisposable
        {
            public TObserver observer;
            public Action<ObserverData> handleDispose;
            public bool disposed { get; private set; }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                handleDispose?.Invoke(this);
            }
        }

        public ObservableBase(SynchronizationContext context = default)
        {
            _context = context ?? SynchronizationContext.Default;
        }

        protected void EnqueueNotify(TNotification notifyData)
        {
            _pendingNotifications.Enqueue(notifyData);
            _context.EnqueueActionImmediate(NotifyNextImmediate);
            _context.EnqueueAction(NotifyNext);
        }

        private void NotifyNext()
        {
            var notification = _pendingNotifications.Dequeue();
            _immediateNotificationIndex--;
            NotifyInternal(notification, _observers);
        }

        private void NotifyNextImmediate()
        {
            var notification = _pendingNotifications.Skip(_immediateNotificationIndex).First();
            _immediateNotificationIndex++;
            NotifyInternal(notification, _immediateObservers);
        }

        private void NotifyInternal(TNotification notification, List<ObserverData> observers)
        {
            _notifyingObservers = true;

            int count = observers.Count;

            for (int i = 0; i < count; i++)
            {
                var instance = observers[i];

                if (instance.disposed)
                    continue;

                NotifyObserver(instance.observer, notification);
            }

            foreach (var disposed in _disposedObservers)
                observers.Remove(disposed);

            _disposedObservers.Clear();

            _notifyingObservers = false;
        }

        protected abstract void NotifyObserver(TObserver observer, TNotification data);

        private void HandleObserverDisposed(ObserverData observerData)
        {
            if (_disposed)
                return;

            if (_notifyingObservers)
            {
                _disposedObservers.Add(observerData);
            }
            else if (observerData.observer.immediate)
            {
                _immediateObservers.Remove(observerData);
            }
            else
            {
                _observers.Remove(observerData);
            }

            observerData.observer.OnDispose();
        }

        protected IDisposable AddObserver(TObserver observer)
        {
            var data = new ObserverData() { observer = observer, handleDispose = HandleObserverDisposed };

            if (observer.immediate)
            {
                _immediateObservers.Add(data);
            }
            else
            {
                _observers.Add(data);
            }

            return data;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var observerData in _observers)
                observerData.observer.OnDispose();

            _observers.Clear();
        }
    }
}