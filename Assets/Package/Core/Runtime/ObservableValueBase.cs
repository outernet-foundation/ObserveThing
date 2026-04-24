using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableValueBase<T> : Observable<T>, IValueObservable<T>
    {
        protected T _value { get; private set; }
        private List<T> _initOperations = new List<T>();

        public ObservableValueBase(ObservationContext context, T value) : base(context)
        {
            _value = value;
            _initOperations.Add(default);
        }

        protected override IReadOnlyList<T> GetInitializationOperations()
        {
            _initOperations[0] = _value;
            return _initOperations;
        }

        protected void SetValueInternal(T value)
        {
            if (Equals(_value, value))
                return;

            _value = value;
            EnqueuePendingOperation(value);
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
            => Subscribe(
                new Observer<T>(
                    onOperation: ops =>
                    {
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