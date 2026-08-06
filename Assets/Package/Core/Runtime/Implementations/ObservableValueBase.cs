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
        public IObservable source { get; set; }
        public T value { get; set; }

        object IValueOp.value => value;
    }

    public class ObservableValueBase<T> : ObservableBase<IValueObserver<T>, ValueOp<T>>, IValueObservable<T>
    {
        protected T _value { get; private set; }

        public ObservableValueBase(ObservationContext context) : this(context, default) { }
        public ObservableValueBase(ObservationContext context, T value) : base(context)
        {
            _value = value;
        }

        protected void SetValueInternal(T value)
        {
            if (Equals(_value, value))
                return;

            _value = value;
            EnqueuePendingOperation(new ValueOp<T>() { source = this, value = value });
        }

        protected override IEnumerable<ValueOp<T>> GetInitializationOperations()
        {
            yield return new ValueOp<T>() { source = this, value = _value };
        }

        protected override void SendOperation(IValueObserver<T> observer, ValueOp<T> operation)
            => observer.OnNext(operation.value);

        public IDisposable Subscribe(IValueObserver<T> observer, bool immediate = false, uint? priority = null)
            => AddObserver(observer, immediate, priority);
    }
}