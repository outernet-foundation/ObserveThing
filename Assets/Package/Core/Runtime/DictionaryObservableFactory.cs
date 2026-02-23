using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class FactoryDictionaryObservable<TKey, TValue> : IDictionaryObservable<TKey, TValue>
    {
        private Func<IDictionaryObserver<TKey, TValue>, IDisposable> _subscribe;

        public FactoryDictionaryObservable(Func<IDictionaryObserver<TKey, TValue>, IDisposable> subscribe)
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

        public IDisposable Subscribe(IObserver observer)
            => _subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (_, _) => observer.OnChange(),
                onRemove: (_, _) => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}