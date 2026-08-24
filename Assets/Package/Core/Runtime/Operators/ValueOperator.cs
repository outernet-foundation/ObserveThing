using System;

namespace ObserveThing
{
    public class ValueOperator<T> : Operator<ObservableValue<T>>, IValueObservable<T>
    {
        public ValueOperator(ObservationContext context, Func<ObservableValue<T>, IDisposable> generateOperator) : base(context, generateOperator) { }

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

        public IDisposable Subscribe(IObserver<IOperation> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((IObservable<IOperation>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }
    }
}