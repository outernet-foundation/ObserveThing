using System;

namespace ObserveThing
{
    public class ValueObservableFactory<T> : IValueObservable<T>
    {
        private Func<IValueObserver<T>, IDisposable> _subscribe;

        public ValueObservableFactory(Func<IValueObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
            => _subscribe(observer);
    }
}