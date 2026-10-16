using System;

namespace ObserveThing
{
    public class CollectionOperatorFactory<T> : OperatorFactory<ObservableCollection<T>>, ICollectionObservable<T>
    {
        public CollectionOperatorFactory(ObservationContext context, Func<ObservableCollection<T>, IDisposable> factory) : base(context, factory) { }

        public IDisposable Subscribe(ICollectionObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((ICollectionObservable<T>)_observable).Subscribe(observer, immediate, priority);
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
