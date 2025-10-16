using System;

namespace ObserveThing
{
    public class IndexOfObservable<T> : IValueObservable<int>
    {
        public IListObservable<T> list;
        public T indexOf;

        public IndexOfObservable(IListObservable<T> list, T indexOf)
        {
            this.list = list;
            this.indexOf = indexOf;
        }

        public IDisposable Subscribe(IObserver<ValueEventArgs<int>> observer)
            => new Instance(this, list, indexOf, observer);

        private class Instance : IDisposable
        {
            private IDisposable _listStream;
            private T _indexOf;
            private IObserver<ValueEventArgs<int>> _observer;
            private ValueEventArgs<int> _args = new ValueEventArgs<int>();
            private bool _disposed = false;

            public Instance(IObservable source, IListObservable<T> list, T indexOf, IObserver<ValueEventArgs<int>> observer)
            {
                _indexOf = indexOf;
                _observer = observer;
                _args.source = source;
                _args.previousValue = -1;
                _args.currentValue = -1;
                _listStream = list.Subscribe(HandleSourceChanged, HandleSourceError, HandleSourceDisposed);
            }

            private void HandleSourceChanged(ListEventArgs<T> args)
            {
                if (Equals(args.element, _indexOf))
                {
                    if (args.operationType == OpType.Add)
                    {
                        _args.previousValue = _args.currentValue;
                        _args.currentValue = args.index;
                        _observer.OnNext(_args);
                    }
                    else
                    {
                        _args.previousValue = _args.currentValue;
                        _args.currentValue = -1;
                        _observer.OnNext(_args);
                    }

                    return;
                }

                if (args.index > _args.currentValue)
                    return;

                _args.previousValue = _args.currentValue;
                _args.currentValue = _args.currentValue++;
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

                _listStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}