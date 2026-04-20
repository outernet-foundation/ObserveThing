using System;

namespace ObserveThing
{
    public class ListObservableFactory<T> : IListObservable<T>
    {
        private Func<IListObserver<T>, IDisposable> _subscribe;

        public ListObservableFactory(Func<IListObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IListObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, _, x) => observer.OnAdd(id, x),
                onRemove: (id, _, x) => observer.OnRemove(id, x),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}