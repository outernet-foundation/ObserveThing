using System;

namespace ObserveThing
{
    public class ValueOperatorFactory<T> : IValueOperator<T>
    {
        private Func<IValueObserver<T>, IDisposable> _subscribe;

        public ValueOperatorFactory(Func<IValueObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
            => _subscribe(observer);
    }
}