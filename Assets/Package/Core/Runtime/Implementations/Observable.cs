using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IOperation
    {
        IObservable source { get; }
        object value { get; }
        IOperation Clone();
    }

    public interface IOperation<T> : IOperation
    {
        new IObservable<T> source { get; }
        new T value { get; }

        IObservable IOperation.source => source;
        object IOperation.value => value;
    }

    public class Operation<T> : IOperation<T>
    {
        public IObservable<T> source { get; }
        public T value { get; set; }

        public Operation(IObservable<T> source)
        {
            this.source = source;
        }

        public IOperation Clone()
        {
            return new Operation<T>(source) { value = value };
        }

        public override string ToString()
        {
            return $"Op[{source}: {value}]";
        }
    }

    public abstract class Observable<T> : IObservable<T>, IDisposable
    {
        private class ObserverData : IPendingObserver, IDisposable
        {
            public IObserver<T> observer { get; }
            public uint priority { get; }
            public bool immediate => observer.immediate;
            public bool pending;
            public bool disposed { get; private set; }

            private List<T> _pendingOperations;
            private List<T> _pendingOperations1 = new List<T>();
            private List<T> _pendingOperations2 = new List<T>();

            private Action<ObserverData> _onDispose;

            public ObserverData(IObserver<T> observer, uint priority, Action<ObserverData> onDispose)
            {
                this.observer = observer;
                this.priority = priority;

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

            public void EnqueuePendingOperation(T operation)
            {
                _pendingOperations.Add(operation);
            }

            public void SendNext()
            {
                if (_pendingOperations.Count == 0)
                    return;

                var ops = _pendingOperations;
                SwitchPendingOperationsList();
                pending = false;

                try
                {
                    observer.OnOperation(ops);
                }
                catch (Exception exc)
                {
                    observer.OnError(exc);
                }

                ops.Clear();
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

        public ObservationContext context { get; }
        public bool disposed { get; private set; }

        private Queue<Operation<T>> _operationPool = new Queue<Operation<T>>();
        private List<Operation<T>> _opList = new List<Operation<T>>();

        private List<ObserverData> _observers = new List<ObserverData>();

        public Observable(ObservationContext context)
        {
            this.context = context ?? Settings.DefaultObservationContext;
        }

        protected void EnqueuePendingOperation(T operation)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            foreach (var observer in _observers)
            {
                observer.EnqueuePendingOperation(operation);

                if (!observer.pending)
                    context.RegisterPendingObserver(observer);
            }

            context.NotifyPendingObserversIfNecessary();
        }

        protected virtual void DisposeInternal() { }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            var observerData = new ObserverData(observer, context.AllocateObserverPriority(), HandleObserverDisposed);
            _observers.Add(observerData);

            observer.OnOperation(null);

            return observerData;
        }

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new Observer<T>(
                onOperation: ops =>
                {
                    if (ops == null)
                    {
                        observer.OnOperation(null);
                        return;
                    }

                    foreach (var op in ops)
                    {
                        if (!_operationPool.TryDequeue(out var operation))
                            operation = new Operation<T>(this);

                        operation.value = op;
                        _opList.Add(operation);
                    }

                    observer.OnOperation(_opList);

                    foreach (var op in _opList)
                    {
                        op.value = default;
                        _operationPool.Enqueue(op);
                    }

                    _opList.Clear();
                },
                observer.OnError,
                observer.OnDispose,
                immediate: observer.immediate
            ));

        private void HandleObserverDisposed(ObserverData data)
        {
            if (disposed)
                return;

            _observers.Remove(data);

            context.DeallocateObserverPriority(data.priority);
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