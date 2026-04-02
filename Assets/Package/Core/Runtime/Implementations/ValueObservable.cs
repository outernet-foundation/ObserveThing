using System;

namespace ObserveThing
{
    public class ValueObservable<T> : ObservableBase<IValueObserver<T>, T>, IValueObservable<T>
    {
        public T value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                _value = value;
                EnqueueNotify(value);
            }
        }

        private T _value = default;

        public ValueObservable(SynchronizationContext context = default) : this(default, context) { }
        public ValueObservable(T startValue, SynchronizationContext context = default) : base(context)
        {
            _value = startValue;
        }

        protected override void NotifyObserver(IValueObserver<T> observer, T data)
        {
            observer.OnNext(data);
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
        {
            var subscription = AddObserver(observer);
            observer.OnNext(value);
            return subscription;
        }

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ValueObserver<T>(
                onNext: _ => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}