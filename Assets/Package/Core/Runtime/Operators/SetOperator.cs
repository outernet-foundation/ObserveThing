using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class SetOperator<T> : ISetOperand<T>, ISetObservable<T>, IObservable<IOperation>, IDisposable
    {
        public ObservationContext context { get; }

        private Func<ISetOperand<T>, IDisposable> _generateOperator;
        private ObservableSet<T> _observable;
        private IDisposable _operator;
        private bool _active = false;
        private List<IObserverBase> _observers = new List<IObserverBase>();
        private bool _disposed;

        public SetOperator(ObservationContext context, Func<ISetOperand<T>, IDisposable> generateOperator)
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

        void ISetOperand<T>.Add(T value)
            => _observable.Add(value);

        void ISetOperand<T>.Remove(T value)
            => _observable.Remove(value);

        void ISetOperand<T>.Clear()
            => _observable.Clear();

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
            _observable = new ObservableSet<T>(context);
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

        public IDisposable Subscribe(ISetObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            HandleObserverAdded(observer);
            var subscription = ((ISetObservable<T>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => HandleObserverRemoved(observer)));
        }

        public IDisposable Subscribe(ISetObserver observer, bool immediate = false, uint? priority = null)
        {
            HandleObserverAdded(observer);
            var subscription = ((ISetObservable)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => HandleObserverRemoved(observer)));
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            HandleObserverAdded(observer);
            var subscription = ((ICollectionObservable<T>)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => HandleObserverRemoved(observer)));
        }

        public IDisposable Subscribe(ICollectionObserver observer, bool immediate = false, uint? priority = null)
        {
            HandleObserverAdded(observer);
            var subscription = ((ICollectionObservable)_observable).Subscribe(observer, immediate, priority);
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
