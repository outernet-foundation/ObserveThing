using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableOperation<T> : IOperation<T>
    {
        public T value { get; set; }
        public IOperationObservable source { get; set; }
    }

    public class ValueObservable<T> : IOperationObservable, IValueObservable<T>, IDisposable
    {
        public T value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                _value = value;
                _context.RegisterOperation(this, value);
            }
        }


        private T _value = default;
        private ObservationContext _context;

        public ValueObservable(ObservationContext context = default) : this(default, context) { }
        public ValueObservable(T startValue, ObservationContext context = default)
        {
            _context = context ?? ObservationContext.Default;
            _value = startValue;
        }

        IDisposable IValueObservable<T>.Subscribe(IValueObserver<T> observer)
            => _context.RegisterObserver(
                new OperationObserver(
                    onNext: ops =>
                    {
                        // init
                        if (ops == null)
                        {
                            observer.OnNext(value);
                            return;
                        }

                        foreach (var op in ops.Cast<IOperation<T>>())
                            observer.OnNext(op.value);
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                ),
                this
            );

        public void Dispose()
        {
            _context.HandleObservableDisposed(this);
        }
    }
}