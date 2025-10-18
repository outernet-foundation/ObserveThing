using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ValueObservable<T> : IValueObservable<T>
    {
        public T value
        {
            get => _value;
            private set
            {
                if (Equals(_value, value))
                    return;

                _args.previousValue = _value;
                _value = value;
                _args.currentValue = value;

                SafeOnNext(_args);
            }
        }

        private T _value = default;
        private ValueEventArgs<T> _args = new ValueEventArgs<T>();
        private List<Instance> _instances = new List<Instance>();
        private List<Instance> _disposedInstances = new List<Instance>();
        private bool _executingOnNext;
        private bool _disposed;
        private IDisposable _fromSubscription;

        public ValueObservable() : this(default) { }
        public ValueObservable(T startValue)
        {
            _value = startValue;
            _args.source = this;
        }

        private void SafeOnNext(ValueEventArgs<T> args)
        {
            _executingOnNext = true;

            int count = _instances.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _instances[i];
                if (instance.disposed)
                    continue;

                instance.OnNext(args);
            }

            foreach (var disposedInstance in _disposedInstances)
                _instances.Remove(disposedInstance);

            _executingOnNext = false;
        }

        public void From(T source)
        {
            _fromSubscription?.Dispose();
            value = source;
        }

        public void From(IValueObservable<T> source)
        {
            _fromSubscription?.Dispose();
            _fromSubscription = source.Subscribe(x => value = x.currentValue);
        }

        public IDisposable Subscribe(IObserver<ValueEventArgs<T>> observer)
        {
            var instance = new Instance(observer, x =>
            {
                if (_disposed)
                    return;

                if (_executingOnNext)
                {
                    _disposedInstances.Add(x);
                    return;
                }

                _instances.Remove(x);
            });

            _instances.Add(instance);

            _args.previousValue = default;
            _args.currentValue = _value;

            instance.OnNext(_args);
            return instance;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _fromSubscription?.Dispose();

            _disposed = true;

            foreach (var instance in _instances)
                instance.Dispose();

            _instances.Clear();
        }

        private class Instance : IDisposable
        {
            public bool disposed { get; private set; }

            private IObserver<ValueEventArgs<T>> _observer;
            private Action<Instance> _onDispose;

            public Instance(IObserver<ValueEventArgs<T>> observer, Action<Instance> onDispose)
            {
                _observer = observer;
                _onDispose = onDispose;
            }

            public void OnNext(ValueEventArgs<T> args)
            {
                _observer?.OnNext(args);
            }

            public void OnError(Exception error)
            {
                _observer?.OnError(error);
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                _observer.OnDispose();
                _observer = null;

                _onDispose(this);
            }
        }
    }
}