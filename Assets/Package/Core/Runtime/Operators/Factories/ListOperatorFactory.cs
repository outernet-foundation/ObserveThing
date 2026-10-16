using System;

namespace ObserveThing
{
    public class ListOperatorFactory<T> : OperatorFactory<ObservableList<T>>, IListObservable<T>
    {
        public ListOperatorFactory(ObservationContext context, Func<ObservableList<T>, IDisposable> factory) : base(context, factory) { }

        public IDisposable Subscribe(IListObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((IListObservable<T>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

        public IDisposable Subscribe(IListObserver observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((IListObservable)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

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
