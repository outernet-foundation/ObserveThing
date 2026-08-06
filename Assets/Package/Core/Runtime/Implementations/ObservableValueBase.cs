using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableValueBase<T> : ObservableBase<IValueObserver<T>, T>, IValueObservable<T>
    {
        protected T _value { get; private set; }
        private Stack<Operation> _operationPool = new Stack<Operation>();

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
            EnqueuePendingOperation(value);
        }

        protected override void SendOperation(IValueObserver<T> observer, T operation)
            => observer.OnNext(operation);

        public IDisposable Subscribe(IValueObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            var subscription = AddObserver(observer, immediate, priority);
            observer.OnNext(_value);
            return subscription;
        }

        public IDisposable Subscribe(IObserver observer, bool immediate = false, uint? priority = null)
            => Subscribe(new ValueObserver<T>(
                onNext: x =>
                {
                    var operation = _operationPool.TryPop(out var op) ? op : new Operation(this);
                    operation.args = x;
                    observer.OnNext(operation);
                    _operationPool.Push(operation);
                },
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}