using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public abstract class ObservableBase<TObserver, TData> : IDisposable where TObserver : IObserverBase
    {
        private SynchronizationContext _context;
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _disposed;

        private Queue<TData> _pendingNotifications = new Queue<TData>();

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

        protected void EnqueueNotify(TData notifyData)
        {
            _pendingNotifications.Enqueue(notifyData);
            _context.EnqueueAction(NotifyNext);
        }

        public void NotifyNext()
        {
            _notifyingObservers = true;

            int count = _observers.Count;
            TData data = _pendingNotifications.Dequeue();

            for (int i = 0; i < count; i++)
            {
                var instance = _observers[i];

                if (instance.disposed)
                    continue;

                NotifyObserver(instance.observer, data);
            }

            foreach (var disposed in _disposedObservers)
                _observers.Remove(disposed);

            _disposedObservers.Clear();

            _notifyingObservers = false;
        }

        protected abstract void NotifyObserver(TObserver observer, TData data);

        private void HandleObserverDisposed(ObserverData observerData)
        {
            if (_disposed)
                return;

            if (_notifyingObservers)
            {
                _disposedObservers.Add(observerData);
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
            _observers.Add(data);
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