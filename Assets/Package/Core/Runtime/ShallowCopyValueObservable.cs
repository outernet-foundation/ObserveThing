using System;

namespace ObserveThing
{
    public class ShallowCopyValueObservable<T> : IValueObservable<T>
    {
        private IValueObservable<IValueObservable<T>> _value;

        public ShallowCopyValueObservable(IValueObservable<IValueObservable<T>> value)
        {
            _value = value;
        }

        public IDisposable Subscribe(IObserver<IValueEventArgs<T>> observer)
            => new Instance(this, _value, observer);

        private class Instance : IDisposable
        {
            private IDisposable _valueStream;
            private IDisposable _nestedValueStream;
            private IObserver<IValueEventArgs<T>> _observer;
            private ValueEventArgs<T> _args = new ValueEventArgs<T>();
            private bool _initializeCalled = false;
            private bool _disposed = false;
            private bool _disposingForSwitch = false;

            public Instance(IObservable source, IValueObservable<IValueObservable<T>> value, IObserver<IValueEventArgs<T>> observer)
            {
                _observer = observer;
                _args.source = source;
                _valueStream = value.Subscribe(HandleSourceChanged, HandleSourceError, HandleSourceDisposed);

                if (!_initializeCalled)
                    _observer.OnNext(_args); // we should always send an initial call, even if there's no change
            }

            private void HandleSourceChanged(IValueEventArgs<IValueObservable<T>> args)
            {
                _disposingForSwitch = true;
                _nestedValueStream?.Dispose();
                _nestedValueStream = null;
                _disposingForSwitch = false;

                if (args.currentValue == default)
                {
                    if (Equals(_args.currentValue, default(T)))
                        return;

                    _args.previousValue = _args.currentValue;
                    _args.currentValue = default;
                    _initializeCalled = true;
                    _observer.OnNext(_args);
                    return;
                }

                _nestedValueStream = args.currentValue.Subscribe(HandleNestedSourceChanged, HandleSourceError, HandleNestedSourceDisposed);
            }

            private void HandleSourceError(Exception error)
            {
                _observer.OnError(error);
            }

            private void HandleSourceDisposed()
            {
                Dispose();
            }

            private void HandleNestedSourceChanged(IValueEventArgs<T> args)
            {
                _args.previousValue = _args.currentValue;
                _args.currentValue = args.currentValue;
                _initializeCalled = true;
                _observer.OnNext(_args);
            }

            private void HandleNestedSourceDisposed()
            {
                if (_disposingForSwitch)
                    return;

                if (Equals(_args.currentValue, default(T)))
                    return;

                _args.previousValue = _args.currentValue;
                _args.currentValue = default;
                _initializeCalled = true;
                _observer.OnNext(_args);
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _valueStream.Dispose();
                _disposingForSwitch = true; // set this to supress the reset call
                _nestedValueStream?.Dispose();
                _observer.OnDispose();
            }
        }
    }
}