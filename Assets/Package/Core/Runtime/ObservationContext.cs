using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IOperation
    {
        IOperationObservable source { get; }
        object value { get; }
    }

    public interface IOperation<out T> : IOperation
    {
        new T value { get; }

        object IOperation.value => value;
    }

    public class ObservationContext
    {
        public static ObservationContext Default { get; } = new ObservationContext();

        private interface IPooledOperation : IOperation
        {
            void Hold();
            void Release();
        }

        private class PooledOperation<T> : IPooledOperation, IOperation<T>
        {
            public IOperationObservable source { get; set; }
            public T value { get; set; }

            private int _holdCount = 0;
            private Action<PooledOperation<T>> _deallocate;

            public PooledOperation(Action<PooledOperation<T>> deallocate)
            {
                _deallocate = deallocate;
            }

            public void Hold()
            {
                _holdCount++;
            }

            public void Release()
            {
                if (_holdCount == 0)
                    return;

                _holdCount--;

                if (_holdCount == 0)
                    _deallocate(this);
            }
        }

        private class OperationPool<T>
        {
            private HashSet<PooledOperation<T>> _allocatedInstances = new HashSet<PooledOperation<T>>();
            private HashSet<PooledOperation<T>> _unallocatedInstances = new HashSet<PooledOperation<T>>();

            public PooledOperation<T> Allocate()
            {
                PooledOperation<T> instance;

                if (_unallocatedInstances.Count > 0)
                {
                    instance = _unallocatedInstances.First();
                    _unallocatedInstances.Remove(instance);
                }
                else
                {
                    instance = new PooledOperation<T>(Deallocate);
                }

                _allocatedInstances.Add(instance);

                return instance;
            }

            public void Deallocate(PooledOperation<T> instance)
            {
                _allocatedInstances.Remove(instance);
                _unallocatedInstances.Add(instance);
            }
        }

        private class ObserverData : IDisposable
        {
            public IOperationObserver observer { get; }
            public List<IOperationObservable> observed { get; } = new List<IOperationObservable>();
            public int order { get; }
            public bool pending;
            public bool disposed { get; private set; }

            private List<IPooledOperation> _pendingOperations;
            private List<IPooledOperation> _pendingOperations1 = new List<IPooledOperation>();
            private List<IPooledOperation> _pendingOperations2 = new List<IPooledOperation>();

            private Action<ObserverData> _onDispose;

            public ObserverData(IOperationObserver observer, int order, Action<ObserverData> onDispose)
            {
                this.observer = observer;
                this.order = order;

                _onDispose = onDispose;

                SwitchPendingOperationsList();
            }

            private void SwitchPendingOperationsList()
            {
                if (_pendingOperations == _pendingOperations1)
                {
                    _pendingOperations = _pendingOperations2;
                }
                else
                {
                    _pendingOperations = _pendingOperations1;
                }
            }

            public void EnqueuePendingOperation(IPooledOperation operation)
            {
                operation.Hold();
                _pendingOperations.Add(operation);
            }

            public void SendNext()
            {
                if (_pendingOperations.Count == 0)
                    return;

                var ops = _pendingOperations;
                SwitchPendingOperationsList();

                try
                {
                    observer.OnNext(ops.AsReadOnly());
                }
                catch (Exception exc)
                {
                    observer.OnError(exc);
                }

                foreach (var operation in ops)
                    operation.Release();

                ops.Clear();
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                foreach (var operation in _pendingOperations)
                    operation.Release();

                _onDispose?.Invoke(this);

                observer.OnDispose();
            }
        }

        private Dictionary<IOperationObserver, ObserverData> _observers = new Dictionary<IOperationObserver, ObserverData>();
        private Dictionary<IOperationObservable, HashSet<ObserverData>> _observersByObservables = new Dictionary<IOperationObservable, HashSet<ObserverData>>();
        private PriorityQueue<ObserverData, int> _pendingImmediateObservers = new PriorityQueue<ObserverData, int>();
        private PriorityQueue<ObserverData, int> _pendingObservers = new PriorityQueue<ObserverData, int>();
        private Dictionary<Type, object> _operationPools = new Dictionary<Type, object>();
        private int _nextObserverOrder = 0;

        private bool _notifyingImmediateObservers = false;
        private bool _notifyingObservers = false;
        private bool _executingBatch = false;

        public void ExecuteBatchOperation(Action batchOperation)
        {
            bool wasExecutingBatch = _executingBatch;
            _executingBatch = true;
            batchOperation.Invoke();
            _executingBatch = wasExecutingBatch;

            if (!_executingBatch && !_notifyingObservers)
                DrainPendingObserverQueue();
        }

        public IDisposable RegisterObserver(IOperationObserver observer, params IOperationObservable[] observables)
        {
            var observerData = new ObserverData(observer, _nextObserverOrder++, HandleObserverDisposed);
            observerData.observed.AddRange(observables);

            _observers.Add(observer, observerData);

            foreach (var observable in observables)
            {
                if (!_observersByObservables.TryGetValue(observable, out var observers))
                {
                    observers = new HashSet<ObserverData>();
                    _observersByObservables.Add(observable, observers);
                }

                observers.Add(observerData);
            }

            // Init
            observer.OnNext(null);

            return observerData;
        }

        public void DeregisterObserver(IOperationObserver observer)
        {
            if (!_observers.TryGetValue(observer, out var observerData))
                return;

            observerData.Dispose();
        }

        private void HandleObserverDisposed(ObserverData data)
        {
            foreach (var observable in data.observed)
            {
                if (!_observersByObservables.TryGetValue(observable, out var observers))
                    continue;

                observers.Remove(data);

                // Nothing is observing this observable anymore
                if (observers.Count == 0)
                    _observersByObservables.Remove(observable);
            }

            _observers.Remove(data.observer);
        }

        public void HandleObservableDisposed(IOperationObservable observable)
        {
            if (!_observersByObservables.TryGetValue(observable, out var observers))
                return;

            _observersByObservables.Remove(observable);

            foreach (var observer in observers.OrderByDescending(x => x.observer.immediate).ThenBy(x => x.order))
            {
                observer.observed.Remove(observable);
                if (observer.observed.Count == 0)
                    observer.Dispose();
            }
        }

        public void RegisterOperation<T>(IOperationObservable observable, T value)
        {
            if (!_observersByObservables.TryGetValue(observable, out var observers))
                return;

            OperationPool<T> operationPool;

            if (_operationPools.TryGetValue(typeof(T), out var poolObject))
            {
                operationPool = (OperationPool<T>)poolObject;
            }
            else
            {
                operationPool = new OperationPool<T>();
                _operationPools.Add(typeof(T), operationPool);
            }

            var operation = operationPool.Allocate();

            operation.source = observable;
            operation.value = value;

            foreach (var observer in observers.Where(x => x.observer.immediate))
            {
                observer.EnqueuePendingOperation(operation);
                if (!observer.pending)
                {
                    observer.pending = true;
                    _pendingImmediateObservers.Enqueue(observer, observer.order);
                }
            }

            if (!_notifyingImmediateObservers)
                DrainPendingImmediateObserverQueue();

            foreach (var observer in observers.Where(x => !x.observer.immediate))
            {
                observer.EnqueuePendingOperation(operation);

                if (!observer.pending)
                {
                    observer.pending = true;
                    _pendingObservers.Enqueue(observer, observer.order);
                }
            }

            if (!_executingBatch && !_notifyingObservers)
                DrainPendingObserverQueue();
        }

        private void DrainPendingObserverQueue()
        {
            _notifyingObservers = true;

            while (_pendingObservers.TryDequeue(out var observer, out var _))
            {
                if (observer.disposed)
                    continue;

                observer.pending = false;
                observer.SendNext();
            }

            _notifyingObservers = false;
        }

        private void DrainPendingImmediateObserverQueue()
        {
            _notifyingImmediateObservers = true;

            while (_pendingImmediateObservers.TryDequeue(out var observer, out var _))
            {
                if (observer.disposed)
                    continue;

                observer.pending = false;
                observer.SendNext();
            }

            _notifyingImmediateObservers = false;
        }
    }
}