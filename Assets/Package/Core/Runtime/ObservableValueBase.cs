using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableValueBase<T> : Observable<IValueObserver<T>, T>, IValueObservable<T>
    {
        private class Operation : IOperation
        {
            public IOperationObservable source { get; set; }
            public object value { get; set; }

            public Operation(IOperationObservable source)
            {
                this.source = source;
            }

            public IOperation Duplicate()
                => new Operation(source) { value = value };
        }

        protected T _value { get; private set; }
        private Stack<Operation> _operations = new Stack<Operation>();

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

        public IDisposable Subscribe(IOperationObserver observer, bool immediate = false, uint? priority = null)
            => Subscribe(new ValueObserver<T>(
                onNext: x =>
                {
                    var operation = _operations.TryPop(out var op) ? op : new Operation(this);
                    operation.value = x;
                    observer.OnNext(operation);
                    _operations.Push(operation);
                },
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ));
    }
}