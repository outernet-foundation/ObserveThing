using System;

namespace ObserveThing
{
    public class FactoryValueObservable<T> : IValueObservable<T>
    {
        private Func<IValueObserver<T>, IDisposable> _subscribe;

        public FactoryValueObservable(Func<IValueObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ValueObserver<T>(
                onNext: _ => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}