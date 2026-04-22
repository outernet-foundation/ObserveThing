using System;

namespace ObserveThing
{
    public class SetObservableFactory<T> : ISetObservable<T>
    {
        private Func<ISetObserver<T>, IDisposable> _subscribe;

        public SetObservableFactory(Func<ISetObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(ISetObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => _subscribe(new SetObserver<T>(
                onAdd: observer.OnAdd,
                onRemove: observer.OnRemove,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}