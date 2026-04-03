using System;

namespace ObserveThing
{
    public class ValueObservable<T> : IValueObservable<T>, IDisposable
    {
        public T value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                _value = value;
                _notificationQueue.EnqueueNotify(value);
            }
        }

        private T _value = default;
        private SynchronizedNotificationQueue<IValueObserver<T>, T> _notificationQueue;

        public ValueObservable(SynchronizationContext context = default) : this(default, context) { }
        public ValueObservable(T startValue, SynchronizationContext context = default)
        {
            _notificationQueue = new SynchronizedNotificationQueue<IValueObserver<T>, T>(NotifyObserver, context);
            _value = startValue;
        }

        private void NotifyObserver(IValueObserver<T> observer, T data)
        {
            observer.OnNext(data);
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
        {
            var subscription = _notificationQueue.AddObserver(observer);
            observer.OnNext(value);
            return subscription;
        }

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ValueObserver<T>(
                onNext: _ => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public void Dispose()
        {
            _notificationQueue.Dispose();
        }
    }
}