using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IObservable<out T> where T : IOperation
    {
        ObservationContext context { get; }
        IDisposable Subscribe(IObserver<T> observer);
        IReadOnlyList<T> GetInitializationOperations();
    }

    public class OperationPool<T> where T : IOperation
    {
        private Stack<T> _pool = new Stack<T>();
        private Func<OperationPool<T>, T> _generateOperation;

        public OperationPool(Func<OperationPool<T>, T> generateOperation)
        {
            _generateOperation = generateOperation;
        }

        public T Allocate() => _pool.TryPop(out var op) ? op : _generateOperation(this);
        public void Deallocate(T operation) => _pool.Push(operation);
    }

    public interface IOperation : IDisposable
    {
        IObservable<IOperation> source { get; }
        object value { get; }

        IOperation Duplicate();
    }

    public interface IOperation<T> : IOperation
    {
        new T value { get; }

        object IOperation.value => value;
    }

    public interface ICollectionOperation : IOperation
    {
        uint elementId { get; }
        OpType opType { get; }
    }

    public interface ICollectionOperation<T> : ICollectionOperation, IOperation<T> { }

    public interface IListOperation : ICollectionOperation
    {
        int index { get; }
    }

    public interface IListOperation<T> : IListOperation, ICollectionOperation<T> { }

    public interface ISetOperation : ICollectionOperation { }
    public interface ISetOperation<T> : ISetOperation, ICollectionOperation<T> { }

    public interface IDictionaryOperation : ICollectionOperation
    {
        object dictionaryKey { get; }
        object dictionaryValue { get; }
    }

    public interface IDictionaryOperation<TKey, TValue> : IDictionaryOperation, ICollectionOperation<KeyValuePair<TKey, TValue>>
    {
        object IDictionaryOperation.dictionaryKey => value.Key;
        object IDictionaryOperation.dictionaryValue => value.Value;
    }

    public abstract class Observable<T> : IObservable<T>, IDisposable where T : IOperation
    {
        private class ObserverData : IPendingObserver, IDisposable
        {
            public IObserver<T> observer { get; }
            public uint priority => observer.overridePriority ?? _priority;
            public bool immediate => observer.immediate;
            public bool disposed { get; private set; }

            private Queue<T> _pendingOperations = new Queue<T>();
            private Action<T> _onOperationSent;
            private Action<ObserverData> _onDispose;

            private uint _priority;

            public ObserverData(IObserver<T> observer, uint priority, Action<T> onOperationSent, Action<ObserverData> onDispose)
            {
                this.observer = observer;

                _priority = priority;
                _onOperationSent = onOperationSent;
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
                finally
                {
                    _onOperationSent?.Invoke(op);
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
        private Dictionary<T, int> _operationReferences = new Dictionary<T, int>();

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

            if (_observers.Count == 0)
                return;

            int referenceCount = 0;

            foreach (var observer in _observers)
            {
                observer.EnqueuePendingOperation(operation);
                context.RegisterPendingObserver(observer);
                referenceCount++;
            }

            _operationReferences[operation] = referenceCount;
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

            var observerData = new ObserverData(observer, context.AllocateObserverPriority(), HandleOperationSent, HandleObserverDisposed);

            // do this after calling OnFirstObserverAdded so any resulting operations won't be queued (they'll be reflected in GetInitializationOperations)
            _observers.Add(observerData);

            foreach (var op in GetInitializationOperations())
            {
                observer.OnNext(op);
                op.Dispose();
            }

            return observerData;
        }

        private void HandleOperationSent(T operation)
        {
            var referenceCount = _operationReferences[operation];
            referenceCount -= 1;

            if (referenceCount == 0)
            {
                _operationReferences.Remove(operation);
                operation.Dispose();
                return;
            }

            _operationReferences[operation] = referenceCount;
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