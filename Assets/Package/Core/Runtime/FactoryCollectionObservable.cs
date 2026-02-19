using System;

namespace ObserveThing
{
    public class FactoryCollectionObservable<T> : ICollectionObservable<T>
    {
        private Func<ICollectionObserver<T>, IDisposable> _subscribe;

        public FactoryCollectionObservable(Func<ICollectionObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new CollectionObserver<T>(
                onAdd: (_, _) => observer.OnChange(),
                onRemove: (_, _) => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}