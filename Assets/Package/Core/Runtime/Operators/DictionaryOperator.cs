using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class DictionaryOperator<TKey, TValue> : Operator<ObservableDictionary<TKey, TValue>>, IDictionaryObservable<TKey, TValue>
    {
        public DictionaryOperator(ObservationContext context, Func<ObservableDictionary<TKey, TValue>, IDisposable> generateOperator) : base(context, generateOperator) { }

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

        public IDisposable Subscribe(IObserver<IOperation> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((IObservable<IOperation>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }
    }
}
