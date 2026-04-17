using System;

namespace ObserveThing
{
    public class CollectionOperatorFactory<T> : ICollectionOperator<T>
    {
        private Func<ICollectionObserver<T>, IDisposable> _subscribe;

        public CollectionOperatorFactory(Func<ICollectionObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(IObserver observer)
        {
            throw new NotImplementedException();
        }
    }
}