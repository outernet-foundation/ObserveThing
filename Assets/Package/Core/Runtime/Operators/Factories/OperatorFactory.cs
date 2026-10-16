using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class OperatorFactory<T> where T : class, IDisposable, new()
    {
        public ObservationContext context { get; }

        private Func<T, IDisposable> _factory;
        protected T _observable { get; private set; }
        private IDisposable _operator;
        private bool _active = false;
        private List<IObserver> _observers = new List<IObserver>();
        private bool _disposed;

        public OperatorFactory(ObservationContext context, Func<T, IDisposable> factory)
        {
            this.context = context;
            _factory = factory;
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
            _observable = new T();
            _operator = _factory(_observable);
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
    }
}
