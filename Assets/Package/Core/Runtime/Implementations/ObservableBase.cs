using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IOperation
    {
        IObservable<IOperation> source { get; }
    }

    public abstract class ObservableBase<TObserver, TOperation> : IObservable<IOperation>, IDisposable
        where TObserver : IObserverBase
        where TOperation : IOperation
    {
        private class ObserverData : IPendingObserver, IDisposable
        {
            public IObserverBase observer { get; }
            public bool immediate { get; }
            public uint priority { get; }
            public bool priorityAllocated { get; }
            public bool disposed { get; private set; }

            private Queue<TOperation> _pendingOperations = new Queue<TOperation>();
            private Action<TOperation> _sendOperation;
            private Action<ObserverData> _onDispose;

            public ObserverData(IObserverBase observer, bool immediate, uint priority, bool priorityAllocated, Action<TOperation> sendOperation, Action<ObserverData> onDispose)
            {
                this.observer = observer;
                this.immediate = immediate;
                this.priority = priority;
                this.priorityAllocated = priorityAllocated;

                _sendOperation = sendOperation;
                _onDispose = onDispose;
            }

            public void EnqueuePendingOperation(TOperation operation)
            {
                _pendingOperations.Enqueue(operation);
            }

            public void SendNext()
            {
                if (_pendingOperations.Count == 0)
                    return;

                try
                {
                    _sendOperation(_pendingOperations.Dequeue());
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

        public ObservableBase(ObservationContext context)
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

            if (data.priorityAllocated)
                context.DeallocateObserverPriority(data.priority);
        }

        protected void EnqueuePendingOperation(TOperation operation)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_observers.Count == 0)
                return;

            foreach (var observer in _observers)
            {
                observer.EnqueuePendingOperation(operation);
                context.RegisterPendingObserver(observer);
            }

            context.NotifyPendingObserversIfNecessary();
        }

        protected virtual void OnFirstObserverAdded() { }
        protected virtual void OnLastObserverRemoved() { }
        protected virtual void DisposeInternal() { }

        public void OnError(Exception error)
        {
            foreach (var observer in _observers.OrderByDescending(x => x.immediate).ThenBy(x => x.priority))
                observer.observer.OnError(error);
        }

        protected abstract IEnumerable<TOperation> GetInitializationOperations();

        protected IDisposable AddObserver(TObserver observer, bool immediate, uint? priority)
            => AddObserverInternal(observer, immediate, priority, op => SendOperation(observer, op));

        private IDisposable AddObserverInternal(IObserverBase observer, bool immediate, uint? priority, Action<TOperation> sendOperation)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_observers.Count == 0)
                OnFirstObserverAdded();

            var observerData = new ObserverData(
                observer,
                immediate,
                priority ?? context.AllocateObserverPriority(),
                priority == null,
                sendOperation,
                HandleObserverDisposed
            );

            // do this after calling OnFirstObserverAdded so any resulting operations won't be queued (they'll be reflected in GetInitializationOperations)
            _observers.Add(observerData);

            foreach (var op in GetInitializationOperations())
                sendOperation(op);

            return observerData;
        }

        protected abstract void SendOperation(TObserver observer, TOperation operation);

        public virtual IDisposable Subscribe(TObserver observer, bool immediate = default, uint? priority = default)
            => AddObserverInternal(observer, immediate, priority, op => SendOperation(observer, op));

        public virtual IDisposable Subscribe(IObserver<IOperation> observer, bool immediate = default, uint? priority = default)
            => AddObserverInternal(observer, immediate, priority, op => observer.OnNext(op));

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