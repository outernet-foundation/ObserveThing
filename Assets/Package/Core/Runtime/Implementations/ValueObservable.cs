using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableOperation<T> : IObservableOperation<T>
    {
        public T value { get; set; }
        public IObservable source { get; set; }
    }

    public class ValueObservable<T> : IObservable, IValueOperator<T>, IDisposable
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
        private List<ObservableOperation<T>> _initOpList = new List<ObservableOperation<T>>();

        public ValueObservable(ObservationContext context = default) : this(default, context) { }
        public ValueObservable(T startValue, ObservationContext context = default)
        {
            _context = context ?? ObservationContext.Default;
            _value = startValue;
            _initOpList.Add(new ObservableOperation<T>() { source = this });
        }

        void IObservable.InitializeObserver(IObserver observer)
        {
            _initOpList[0].value = value;
            observer.OnNext(_initOpList);
        }

        IDisposable IValueOperator<T>.Subscribe(IValueObserver<T> observer)
            => _context.RegisterObserver(
                new Observer(
                    onNext: ops =>
                    {
                        foreach (var op in ops.Cast<IObservableOperation<T>>())
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