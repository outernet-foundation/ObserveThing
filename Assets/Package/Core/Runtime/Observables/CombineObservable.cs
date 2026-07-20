using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IOperation : IPooledOperation
    {
        ObservationContext context { get; }
        object source { get; }
        object value { get; }
        bool disposed { get; }
    }

    public interface IOperation<out T> : IOperation
    {
        new IObservable<T> source { get; }
        new T value { get; }

        ObservationContext IOperation.context => source.context;
        object IOperation.source => source;
        object IOperation.value => value;
    }

    public interface IPooledOperation
    {
        void Deallocate();
    }

    public class Operation<T> : IOperation<T>
    {
        public IObservable<T> source { get; }
        public T value { get; set; }
        public bool disposed { get; set; }

        private OperationPool<T> _pool;

        public Operation(IObservable<T> source, OperationPool<T> pool)
        {
            this.source = source;
            _pool = pool;
        }

        public void Deallocate()
        {
            _pool.Deallocate(this);
        }
    }

    public class OperationPool<T>
    {
        private IObservable<T> _source;
        private Queue<Operation<T>> _unallocatedInstance = new Queue<Operation<T>>();

        public OperationPool(IObservable<T> source)
        {
            _source = source;
        }

        public IOperation<T> Allocate(T operation, bool disposed = false)
        {
            if (!_unallocatedInstance.TryDequeue(out var op))
                op = new Operation<T>(_source, this);

            op.value = operation;
            op.disposed = disposed;
            return op;
        }

        public void Deallocate(Operation<T> operation)
        {
            operation.value = default;
            _unallocatedInstance.Enqueue(operation);
        }
    }

    public class CombineObservable<T1, T2> : IInitializationOperationsProvider<IOperation>
    {
        private IObservableOperand<IOperation> _operand;
        private uint _elementPriority;
        private IDisposable _subscriptions;

        private IObservable<T1> _source1;
        private IObservable<T2> _source2;

        private OperationPool<T1> _pool1;
        private OperationPool<T2> _pool2;

        public CombineObservable(IObservable<T1> source1, IObservable<T2> source2, IObservableOperand<IOperation> operand)
        {
            _source1 = source1;
            _source2 = source2;

            _pool1 = new OperationPool<T1>(source1);
            _pool2 = new OperationPool<T2>(source2);

            _operand = operand;
            _elementPriority = _source1.context.AllocateObserverPriority();
            _subscriptions = new ComposedDisposable(

                _source1.Subscribe(
                    onNext: op => operand.EnqueuePendingOperation(_pool1.Allocate(op)),
                    onError: operand.OnError,
                    onDispose: () => operand.EnqueuePendingOperation(_pool1.Allocate(default, disposed: true))
                ),

                _source2.Subscribe(
                    onNext: op => operand.EnqueuePendingOperation(_pool2.Allocate(op)),
                    onError: operand.OnError,
                    onDispose: () => operand.EnqueuePendingOperation(_pool2.Allocate(default, disposed: true))
                )

            );
        }

        public IReadOnlyList<IOperation> GetInitializationOperations()
        {
            return _source1.GetInitializationOperations().Select(x => (IOperation)_pool1.Allocate(x))
                .Concat(_source2.GetInitializationOperations().Select(x => (IOperation)_pool2.Allocate(x)))
                .ToArray();
        }

        public void Dispose()
        {
            _source1.context.DeallocateObserverPriority(_elementPriority);

            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }

    public class CombineObservable<T1, T2, T3> : IInitializationOperationsProvider<IOperation>
    {
        private IObservableOperand<IOperation> _operand;
        private uint _elementPriority;
        private IDisposable _subscriptions;

        private IObservable<T1> _source1;
        private IObservable<T2> _source2;
        private IObservable<T3> _source3;

        private OperationPool<T1> _pool1;
        private OperationPool<T2> _pool2;
        private OperationPool<T3> _pool3;

        public CombineObservable(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservableOperand<IOperation> operand, bool disposeOnSourceEmpty = false)
        {
            _source1 = source1;
            _source2 = source2;
            _source3 = source3;

            _pool1 = new OperationPool<T1>(source1);
            _pool2 = new OperationPool<T2>(source2);
            _pool3 = new OperationPool<T3>(source3);

            _operand = operand;
            _elementPriority = _source1.context.AllocateObserverPriority();
            _subscriptions = new ComposedDisposable(

                _source1.Subscribe(
                    onNext: op => operand.EnqueuePendingOperation(_pool1.Allocate(op)),
                    onError: operand.OnError,
                    onDispose: () => operand.EnqueuePendingOperation(_pool1.Allocate(default, disposed: true))
                ),

                _source2.Subscribe(
                    onNext: op => operand.EnqueuePendingOperation(_pool2.Allocate(op)),
                    onError: operand.OnError,
                    onDispose: () => operand.EnqueuePendingOperation(_pool2.Allocate(default, disposed: true))
                ),

                _source3.Subscribe(
                    onNext: op => operand.EnqueuePendingOperation(_pool3.Allocate(op)),
                    onError: operand.OnError,
                    onDispose: () => operand.EnqueuePendingOperation(_pool3.Allocate(default, disposed: true))
                )

            );
        }

        public IReadOnlyList<IOperation> GetInitializationOperations()
        {
            return _source1.GetInitializationOperations().Select(x => (IOperation)_pool1.Allocate(x))
                .Concat(_source2.GetInitializationOperations().Select(x => (IOperation)_pool2.Allocate(x)))
                .ToArray();
        }

        public void Dispose()
        {
            _source1.context.DeallocateObserverPriority(_elementPriority);

            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }
}