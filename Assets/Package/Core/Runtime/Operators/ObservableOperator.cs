using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableOperator<T> : IObservable<T>, IInitializationOperationsProvider<T> where T : IOperation
    {
        public ObservationContext context { get; }

        private Func<Observable<T>, IInitializationOperationsProvider<T>> _generateOperator;
        protected Observable<T> _observable { get; private set; }
        private IInitializationOperationsProvider<T> _operator;
        private bool _active = false;
        private List<IObserverBase> _observers = new List<IObserverBase>();
        private bool _disposed;

        public ObservableOperator(ObservationContext context, Func<Observable<T>, IInitializationOperationsProvider<T>> generateOperator)
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

        protected void AddObserver(IObserverBase observer)
        {
            _observers.Add(observer);

            if (_active)
                return;

            _active = true;
            _observable = new Observable<T>(context, this);
            _operator = _generateOperator(_observable);
        }

        protected void RemoveObserver(IObserverBase observer)
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

        public IDisposable Subscribe(IObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            AddObserver(observer);
            var subscription = _observable.Subscribe(observer, immediate, priority);
            return new ComposedDisposable(subscription, new Disposable(() => RemoveObserver(observer)));
        }

        public IEnumerable<T> GetInitializationOperations()
            => _operator.GetInitializationOperations();
    }
}