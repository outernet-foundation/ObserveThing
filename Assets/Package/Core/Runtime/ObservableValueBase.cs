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

            public IOperation AllocateCopy()
            {
                return new ValueOperation()
                {
                    source = source,
                    value = value,
                };
            }

            public void Deallocate()
            {
                var context = source.context;
                source = default;
                value = default;
                context.DeallocateOperation(this);
            }
        }

        protected T _value { get; private set; }
        private ValueOperation[] _initOperations = new ValueOperation[1];

        public ObservableValueBase(ObservationContext context) : this(context, default) { }
        public ObservableValueBase(ObservationContext context, T value) : base(context)
        {
            _value = value;
        }

        private ValueOperation AllocateOperation(T value)
        {
            var op = context.AllocateOperation<ValueOperation>();
            op.source = this;
            op.value = value;
            return op;
        }

        public override IReadOnlyList<IValueOperation<T>> GetInitializationOperations()
        {
            var op = context.AllocateOperation<ValueOperation>();
            op.source = this;
            op.value = _value;
            _initOperations[0] = op;
            return _initOperations;
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