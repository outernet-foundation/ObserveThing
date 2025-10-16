using System;

namespace ObserveThing
{
    public class SelectValueObservableReactive<T, U> : IValueObservable<U>
    {
        public IValueObservable<T> value;
        public Func<T, IValueObservable<U>> select;

        public SelectValueObservableReactive(IValueObservable<T> value, Func<T, IValueObservable<U>> select)
        {
            this.value = value;
            this.select = select;
        }

        public IDisposable Subscribe(IObserver<ValueEventArgs<U>> observer)
            => new Instance(this, value, select, observer);

        private class Instance : IDisposable
        {
            private IDisposable _valueStream;
            private IDisposable _nestedSource;
            private Func<T, IValueObservable<U>> _select;
            private IObserver<ValueEventArgs<U>> _observer;
            private ValueEventArgs<U> _args = new ValueEventArgs<U>();
            private bool _initializeCalled = false;
            private bool _disposed = false;

            public Instance(IObservable source, IValueObservable<T> value, Func<T, IValueObservable<U>> select, IObserver<ValueEventArgs<U>> observer)
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
                _nestedSource?.Dispose();
                _nestedSource = _select(args.currentValue).Subscribe(HandleNestedSourceChanged, HandleSourceError, HandleNestedSourceDisposed);
            }

            private void HandleSourceError(Exception exception)
            {
                _observer.OnError(exception);
            }

            private void HandleSourceDisposed()
            {
                Dispose();
            }

            private void HandleNestedSourceChanged(ValueEventArgs<U> args)
            {
                _args.previousValue = _args.currentValue;
                _args.currentValue = args.currentValue;

                if (Equals(_args.currentValue, _args.previousValue))
                    return;

                _initializeCalled = true;
                _observer.OnNext(_args);
            }

            private void HandleNestedSourceDisposed()
            {
                _nestedSource = null;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _nestedSource?.Dispose();
                _valueStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}