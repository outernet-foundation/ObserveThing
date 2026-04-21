using System;
using System.Linq;

namespace ObserveThing
{
    public class ValueObservable<T> : Observable<T>, IValueObservable<T>, IDisposable
    {
        public T value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                _value = value;
                EnqueuePendingOperation(value);
            }
        }

        private T _value = default;

        public ValueObservable(ObservationContext context = default) : this(default, context) { }
        public ValueObservable(T startValue, ObservationContext context = default) : base(context)
        {
            _value = startValue;
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
            => Subscribe(
                new Observer<T>(
                    onOperation: ops =>
                    {
                        // init
                        if (ops == null)
                        {
                            observer.OnNext(value);
                            return;
                        }

                        foreach (var op in ops)
                            observer.OnNext(op);
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                )
            );
    }
}