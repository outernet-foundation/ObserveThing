using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class EventStreamOperator<T> : IEventStreamObservable<T>, IInitializationOperationsProvider<T>
    {
        public ObservationContext context { get; }

        private Func<EventStreamObservable<T>, IInitializationOperationsProvider<T>> _generateOperator;
        protected EventStreamObservable<T> _observable { get; private set; }
        private IInitializationOperationsProvider<T> _operator;
        private bool _active = false;
        private List<IObserver> _observers = new List<IObserver>();
        private bool _disposed;

        public EventStreamOperator(ObservationContext context, Func<EventStreamObservable<T>, IInitializationOperationsProvider<T>> generateOperator)
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

        protected void AddObserver(IObserver observer)
        {
            _observers.Add(observer);

            if (_active)
                return;

            _active = true;
            _observable = new EventStreamObservable<T>(context, this);
            _operator = _generateOperator(_observable);
        }

        protected void RemoveObserver(IObserver observer)
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

        public IDisposable Subscribe(IEventStreamObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = _observable.Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

        public IDisposable Subscribe(IEventStreamObserver observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = ((IEventStreamObservable)_observable).Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

        public IEnumerable<T> GetInitializationOperations()
            => _operator.GetInitializationOperations();
    }
}