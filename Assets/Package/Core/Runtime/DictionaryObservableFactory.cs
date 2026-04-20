using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class DictionaryObservableFactory<TKey, TValue> : IDictionaryObservable<TKey, TValue>
    {
        private Func<IDictionaryObserver<TKey, TValue>, IDisposable> _subscribe;

        public DictionaryObservableFactory(Func<IDictionaryObserver<TKey, TValue>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer)
            => _subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: observer.OnAdd,
                onRemove: observer.OnRemove,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}