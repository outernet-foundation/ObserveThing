using System;

namespace ObserveThing
{
    public class CollectionObservableFactory<T> : ICollectionObservable<T>
    {
        private Func<ICollectionObserver<T>, IDisposable> _subscribe;

        public CollectionObservableFactory(Func<ICollectionObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(IOperationObserver observer)
        {
            throw new NotImplementedException();
        }
    }
}