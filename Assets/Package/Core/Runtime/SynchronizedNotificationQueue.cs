using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public delegate void NotifyObserverDelegate<TObserver, TNotification>(TObserver observer, TNotification notification);

    public class SynchronizedNotificationQueue<TObserver, TNotification> : IDisposable where TObserver : IObserverBase
    {
        private NotifyObserverDelegate<TObserver, TNotification> _notifyObserver;
        private SynchronizationContext _context;
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _immediateObservers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _disposed;

        private Queue<TNotification> _pendingNotifications = new Queue<TNotification>();
        private Queue<ObserverData> _uninitializedObservers = new Queue<ObserverData>();

        private class ObserverData : IDisposable
        {
            public TObserver observer;
            public Action<ObserverData> handleDispose;
            public bool initialized;
            public bool disposed { get; private set; }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                handleDispose?.Invoke(this);
            }
        }

        public SynchronizedNotificationQueue(NotifyObserverDelegate<TObserver, TNotification> notifyObserver, SynchronizationContext context = default)
        {
            _notifyObserver = notifyObserver;
            _context = context ?? SynchronizationContext.Default;
        }

        private void InitializeNextObserver()
        {
            _uninitializedObservers.Dequeue().initialized = true;
        }

        private void NotifyNext()
        {
            NotifyInternal(_pendingNotifications.Dequeue(), _observers, false);
        }

        private void NotifyInternal(TNotification notification, List<ObserverData> observers, bool notifyUninitializedObservers)
        {
            _notifyingObservers = true;

            int count = observers.Count;

            for (int i = 0; i < count; i++)
            {
                var instance = observers[i];

                if ((!notifyUninitializedObservers && !instance.initialized) || instance.disposed)
                    continue;

                _notifyObserver(instance.observer, notification);
            }

            foreach (var disposed in _disposedObservers)
                observers.Remove(disposed);

            _disposedObservers.Clear();

            _notifyingObservers = false;
        }

        private void NotifyDisposeInternal(List<ObserverData> observers)
        {
            _notifyingObservers = true;

            for (int i = 0; i < observers.Count; i++)
            {
                var instance = observers[i];

                if (instance.disposed)
                    continue;

                instance.observer.OnDispose();
            }

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

        private void NotifyDispose()
        {
            NotifyDisposeInternal(_observers);
        }

        public void EnqueueNotify(TNotification notifyData)
        {
            _pendingNotifications.Enqueue(notifyData);

            _context.PauseExecution();
            _context.EnqueueAction(NotifyNext);

            try
            {
                NotifyInternal(notifyData, _immediateObservers, true);
            }
            catch (Exception exc)
            {
                UnityEngine.Debug.LogException(exc);
            }

            _context.ResumeExecution();
        }

        public IDisposable AddObserver(TObserver observer)
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

            _uninitializedObservers.Enqueue(data);
            _context.EnqueueAction(InitializeNextObserver);

            return data;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _context.PauseExecution();
            _context.EnqueueAction(NotifyDispose);

            try
            {
                NotifyDisposeInternal(_immediateObservers);
            }
            catch (Exception exc)
            {
                UnityEngine.Debug.LogException(exc);
            }

            _context.ResumeExecution();
        }
    }
}