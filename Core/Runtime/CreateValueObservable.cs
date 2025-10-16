using System;

namespace ObserveThing
{
    public class CreateValueObservable<T, U> : IValueObservable<U> where U : IDisposable
    {
        public IValueObservable<T> value;
        public Func<T, U> select;

        public CreateValueObservable(IValueObservable<T> value, Func<T, U> select)
        {
            this.value = value;
            this.select = select;
        }

        public IDisposable Subscribe(IObserver<ValueEventArgs<U>> observer)
            => new Instance(this, value, select, observer);

        private class Instance : IDisposable
        {
            private IDisposable _valueStream;
            private Func<T, U> _select;
            private IObserver<ValueEventArgs<U>> _observer;
            private ValueEventArgs<U> _args = new ValueEventArgs<U>();
            private bool _initializeCalled = false;
            private bool _disposed = false;

            public Instance(IObservable source, IValueObservable<T> value, Func<T, U> select, IObserver<ValueEventArgs<U>> observer)
            {
                _select = select;
                _observer = observer;
                _args.source = source;
                _valueStream = value.Subscribe(HandleSourceChanged, HandleSourceError, HandleSourceDisposed);

                if (!_initializeCalled)
                    _observer.OnNext(_args); // we should always send an initial call, even if there's no change
            }

            private void HandleSourceChanged(ValueEventArgs<T> args)
            {
                _args.previousValue = _args.currentValue;
                _args.currentValue = _select(args.currentValue);

                if (Equals(_args.currentValue, _args.previousValue))
                    return;

                _initializeCalled = true;
                _observer.OnNext(_args);
                _args.previousValue?.Dispose();
            }

            private void HandleSourceError(Exception error)
            {
                _observer.OnError(error);
            }

            private void HandleSourceDisposed()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _args.previousValue?.Dispose();
                _args.currentValue?.Dispose();
                _valueStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}