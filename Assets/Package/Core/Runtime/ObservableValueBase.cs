using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IValueOperation : IOperation
    {
        object value { get; }
    }

    public interface IValueOperation<out T> : IValueOperation
    {
        new T value { get; }
        object IValueOperation.value => value;
    }

    public class ObservableValueBase<T> : Observable<IValueOperation<T>>, IValueObservable<T>
    {
        private class ValueOperation : IValueOperation<T>
        {
            public IObservable source { get; set; }
            public T value { get; set; }

            public void Reset()
            {
                source = default;
                value = default;
            }

            public IOperation Clone()
            {
                return new ValueOperation()
                {
                    source = source,
                    value = value,
                };
            }
        }

        protected T _value { get; private set; }
        private List<ValueOperation> _initOperations = new List<ValueOperation>();

        public ObservableValueBase(ObservationContext context) : this(context, default) { }
        public ObservableValueBase(ObservationContext context, T value) : base(context)
        {
            _value = value;
            _initOperations.Add(new ValueOperation() { source = this });
        }

        private ValueOperation AllocateOperation(T value)
        {
            var op = context.AllocatePooledOperation<ValueOperation>();
            op.source = this;
            op.value = value;
            return op;
        }

        protected override IReadOnlyList<IValueOperation<T>> GetInitializationOperations()
        {
            _initOperations[0].value = _value;
            return _initOperations;
        }

        protected override void OnOperationNotificationsCompleted(IValueOperation<T> operation)
        {
            var op = (ValueOperation)operation;
            op.Reset();
            context.DeallocatePooledOperation(op);
        }

        protected void SetValueInternal(T value)
        {
            if (Equals(_value, value))
                return;

            _value = value;
            EnqueuePendingOperation(AllocateOperation(value));
        }

        public IDisposable Subscribe(IObserver<IValueOperation> observer)
            => Subscribe(new Observer<IValueOperation<T>>(
                overridePriority: observer.overridePriority,
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}