using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class DictionaryOperatorFactory<TKey, TValue> : OperatorFactory<ObservableDictionary<TKey, TValue>>, IDictionaryObservable<TKey, TValue>
    {
        public DictionaryOperatorFactory(ObservationContext context, Func<ObservableDictionary<TKey, TValue>, IDisposable> factory) : base(context, factory) { }

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((IDictionaryObservable<TKey, TValue>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

        public IDisposable Subscribe(IDictionaryObserver observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((IDictionaryObservable)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

        public IDisposable Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((ICollectionObservable<KeyValuePair<TKey, TValue>>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

        public IDisposable Subscribe(ICollectionObserver observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((ICollectionObservable)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }
    }
}
