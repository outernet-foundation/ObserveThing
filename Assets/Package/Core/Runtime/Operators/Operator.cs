using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class Operator<T> where T : class, IDisposable, new()
    {
        public ObservationContext context { get; }

        private Func<T, IDisposable> _generateOperator;
        protected T _observable { get; private set; }
        private IDisposable _operator;
        private bool _active = false;
        private List<IObserverBase> _observers = new List<IObserverBase>();
        private bool _disposed;

        public Operator(ObservationContext context, Func<T, IDisposable> generateOperator)
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
            _observable = new T();
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
    }
}
