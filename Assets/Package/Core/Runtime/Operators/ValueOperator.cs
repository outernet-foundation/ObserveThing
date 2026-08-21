using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ValueOperator<T> : IValueOperand<T>, IValueObservable<T>, IObservable<IOperation>, IDisposable
    {
        T IValueOperand<T>.value
        {
            get => _observable.value;
            set => _observable.value = value;
        }

        public ObservationContext context { get; }

        private Func<IValueOperand<T>, IDisposable> _generateOperator;
        private ObservableValue<T> _observable;
        private IDisposable _operator;
        private bool _active = false;
        private List<IObserverBase> _observers = new List<IObserverBase>();
        private bool _disposed;

        public ValueOperator(ObservationContext context, Func<IValueOperand<T>, IDisposable> generateOperator)
        {
            this.context = context;
            _generateOperator = generateOperator;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _operator?.Dispose();
            _operator = default;

            _observable?.Dispose();
            _observable = default;
        }

        void IOperand.OnError(Exception error)
            => _observable.NotifyError(error);

        void IOperand.OnDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        private void HandleObserverAdded(IObserverBase observer)
        {
            _observers.Add(observer);

            if (_active)
                return;

            _active = true;
            _observable = new ObservableValue<T>(context);
            _operator = _generateOperator(this);
        }

        private void HandleObserverRemoved(IObserverBase observer)
        {
            _observers.Remove(observer);

            if (_active && _observers.Count == 0)
            {
                _active = false;

                _operator.Dispose();
                _operator = default;

                _observable.Dispose();
                _observable = default;
            }
        }

        public IDisposable Subscribe(IValueObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            HandleObserverAdded(observer);
            var subscription = ((IValueObservable<T>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => HandleObserverRemoved(observer)));
        }

        public IDisposable Subscribe(IValueObserver observer, bool immediate = false, uint? priority = null)
        {
            HandleObserverAdded(observer);
            var subscription = ((IValueObservable)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => HandleObserverRemoved(observer)));
        }

        public IDisposable Subscribe(IObserver<IOperation> observer, bool immediate = false, uint? priority = null)
        {
            HandleObserverAdded(observer);
            var subscription = ((IObservable<IOperation>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => HandleObserverRemoved(observer)));
        }
    }
}