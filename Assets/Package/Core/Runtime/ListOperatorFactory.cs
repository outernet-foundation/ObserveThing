using System;

namespace ObserveThing
{
    public class ListOperatorFactory<T> : IListOperator<T>
    {
        private Func<IListObserver<T>, IDisposable> _subscribe;

        public ListOperatorFactory(Func<IListObserver<T>, IDisposable> subscribe)
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