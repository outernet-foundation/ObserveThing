using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IValueOp : IOperation
    {
        object value { get; }
    }

    public struct ValueOp<T> : IValueOp
    {
        public IObservable<IOperation> source { get; set; }
        public T value { get; set; }

        object IValueOp.value => value;
    }

    public class ObservableValue<T> : ObservableBase<IValueObserver<T>, ValueOp<T>>, IValueObservable<T>
    {
        public T value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                _value = value;
                EnqueuePendingOperation(new ValueOp<T>() { source = this, value = value });
            }
        }

        private T _value;

        public ObservableValue() : this(default, default) { }
        public ObservableValue(T value) : this(default, value) { }
        public ObservableValue(ObservationContext context) : this(context, default) { }
        public ObservableValue(ObservationContext context, T value) : base(context)
        {
            _value = value;
        }

        protected override IEnumerable<ValueOp<T>> GetInitializationOperations()
        {
            yield return new ValueOp<T>() { source = this, value = _value };
        }

        protected override void SendOperation(IValueObserver<T> observer, ValueOp<T> operation)
            => observer.OnNext(operation.value);

        public IDisposable Subscribe(IValueObserver observer, bool immediate = false, uint? priority = null)
            => Subscribe(new ValueObserver<T>(
                onNext: x => observer.OnNext(x),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}