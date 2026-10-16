using System;

namespace ObserveThing
{
    public class ValueOperatorFactory<T> : OperatorFactory<ObservableValue<T>>, IValueObservable<T>
    {
        public ValueOperatorFactory(ObservationContext context, Func<ObservableValue<T>, IDisposable> factory) : base(context, factory) { }

        public IDisposable Subscribe(IValueObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((IValueObservable<T>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

        public IDisposable Subscribe(IValueObserver observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((IValueObservable)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }
    }
}