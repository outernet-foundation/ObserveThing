using System;

namespace ObserveThing
{
    public class SetOperatorFactory<T> : OperatorFactory<ObservableSet<T>>, ISetObservable<T>
    {
        public SetOperatorFactory(ObservationContext context, Func<ObservableSet<T>, IDisposable> factory) : base(context, factory) { }

        public IDisposable Subscribe(ISetObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((ISetObservable<T>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

        public IDisposable Subscribe(ISetObserver observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((ISetObservable)_observable).Subscribe(observer, immediate, priority);
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
