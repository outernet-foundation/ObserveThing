using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IObservable<out T>
    {
        ObservationContext context { get; }
        IDisposable Subscribe(IObserver<T> observer);
        IReadOnlyList<T> GetInitializationOperations();
    }

    public abstract class Observable<T> : IObservable<T>, IDisposable
    {
        private class ObserverData : IPendingObserver, IDisposable
        {
            public IObserver<T> observer { get; }
            public uint priority => observer.overridePriority ?? _priority;
            public bool immediate => observer.immediate;
            public bool disposed { get; private set; }

            private Queue<T> _pendingOperations = new Queue<T>();

            private Action<ObserverData> _onDispose;

            private uint _priority;

            public ObserverData(IObserver<T> observer, uint priority, Action<ObserverData> onDispose)
            {
                this.observer = observer;

                _priority = priority;
                _onDispose = onDispose;
            }

            public void EnqueuePendingOperation(T operation)
            {
                _pendingOperations.Enqueue(operation);
            }

            public void SendNext()
            {
                if (_pendingOperations.Count == 0)
                    return;

                var op = _pendingOperations.Dequeue();

                try
                {
                    observer.OnNext(op);
                }
                catch (Exception exc)
                {
                    observer.OnError(exc);
                }
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                _onDispose?.Invoke(this);

                observer.OnDispose();
            }
        }

        public ObservationContext context { get; protected set; }
        public bool disposed { get; private set; }

        private List<ObserverData> _observers = new List<ObserverData>();

        public Observable(ObservationContext context)
        {
            this.context = context ?? Settings.DefaultObservationContext;
        }

        private void HandleObserverDisposed(ObserverData data)
        {
            if (disposed)
                return;

            if (!_observers.Remove(data))
                return;

            if (_observers.Count == 0)
                OnLastObserverRemoved();

            context.DeallocateObserverPriority(data.priority);
        }

        protected void EnqueuePendingOperation(T operation)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            int referenceCount = 0;

            foreach (var observer in _observers)
            {
                observer.EnqueuePendingOperation(operation);
                context.RegisterPendingObserver(observer);
                referenceCount++;
            }

            context.NotifyPendingObserversIfNecessary();
        }

        protected virtual void OnFirstObserverAdded() { }
        protected virtual void OnLastObserverRemoved() { }
        protected virtual void DisposeInternal() { }

        protected void OnError(Exception error)
        {
            foreach (var observer in _observers.OrderByDescending(x => x.immediate).ThenBy(x => x.priority))
                observer.observer.OnError(error);
        }

        public abstract IReadOnlyList<T> GetInitializationOperations();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (disposed)
            {
                var disposed = new Disposable(observer.OnDispose);
                disposed.Dispose();
                return disposed;
            }

            if (_observers.Count == 0)
                OnFirstObserverAdded();

            var observerData = new ObserverData(observer, context.AllocateObserverPriority(), HandleObserverDisposed);

            // do this after calling OnFirstObserverAdded so any resulting operations won't be queued (they'll be reflected in GetInitializationOperations)
            _observers.Add(observerData);

            foreach (var op in GetInitializationOperations())
                observer.OnNext(op);

            return observerData;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            foreach (var observer in _observers.OrderByDescending(x => x.immediate).ThenBy(x => x.priority))
            {
                observer.Dispose();
                context.DeallocateObserverPriority(observer.priority);
            }

            _observers.Clear();

            DisposeInternal();
        }
    }
}