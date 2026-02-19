using System;

namespace ObserveThing
{
    public class FactoryListObservable<T> : IListObservable<T>
    {
        private Func<IListObserver<T>, IDisposable> _subscribe;

        public FactoryListObservable(Func<IListObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IListObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (_, _, _) => observer.OnChange(),
                onRemove: (_, _, _) => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, _, x) => observer.OnAdd(id, x),
                onRemove: (id, _, x) => observer.OnRemove(id, x),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}