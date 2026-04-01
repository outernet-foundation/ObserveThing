using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableBase<T> : IDisposable where T : IObserverBase
    {
        private SynchronizationContext _context;
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _disposed;

        private class ObserverData : IDisposable
        {
            public T observer;
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

        protected void EnqueueNotify(Action<T> notifyObserver)
        {
            _context.EnqueueAction(() => NotifyInternal(notifyObserver));
        }

        private void NotifyInternal(Action<T> notifyObserver)
        {
            _notifyingObservers = true;

            int count = _observers.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _observers[i];

                if (instance.disposed)
                    continue;

                notifyObserver(instance.observer);
            }

            foreach (var disposed in _disposedObservers)
                _observers.Remove(disposed);

            _disposedObservers.Clear();

            _notifyingObservers = false;
        }

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

        protected IDisposable AddObserver(T observer)
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