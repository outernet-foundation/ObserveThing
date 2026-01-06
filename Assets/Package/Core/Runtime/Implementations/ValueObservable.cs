using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ValueObservable<T> : IValueObservable<T>
    {
        public T value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                var previousValue = _value;
                _value = value;

                SafeOnNext(this, previousValue, _value);
            }
        }

        private T _value = default;
        private List<Instance> _instances = new List<Instance>();
        private List<Instance> _disposedInstances = new List<Instance>();
        private bool _executingOnNext;
        private bool _disposed;

        public ValueObservable() : this(default) { }
        public ValueObservable(T startValue)
        {
            _value = startValue;
        }

        private void SafeOnNext(IObservable source, T previousValue, T currentValue)
        {
            _executingOnNext = true;

            int count = _instances.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _instances[i];
                if (instance.disposed)
                    continue;

                instance.OnNext(source, previousValue, currentValue);
            }

            foreach (var disposedInstance in _disposedInstances)
                _instances.Remove(disposedInstance);

            _executingOnNext = false;
        }

        public IDisposable Subscribe(IObserver<IValueEventArgs<T>> observer)
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
            instance.OnNext(this, default, _value);
            return instance;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var instance in _instances)
                instance.Dispose();

            _instances.Clear();
        }

        private class Instance : IDisposable
        {
            public bool disposed { get; private set; }

            private IObserver<IValueEventArgs<T>> _observer;
            private Action<Instance> _onDispose;
            private ValueEventArgs<T> _args = new ValueEventArgs<T>();

            public Instance(IObserver<IValueEventArgs<T>> observer, Action<Instance> onDispose)
            {
                _observer = observer;
                _onDispose = onDispose;
            }

            public void OnNext(IObservable source, T previousValue, T currentValue)
            {
                _args.source = source;
                _args.previousValue = previousValue;
                _args.currentValue = currentValue;
                _observer?.OnNext(_args);
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