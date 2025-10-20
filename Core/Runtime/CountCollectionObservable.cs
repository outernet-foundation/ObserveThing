using System;

namespace ObserveThing
{
    public class CountCollectionObservable<T> : IValueObservable<int>
    {
        public ICollectionObservable<T> collection;

        public CountCollectionObservable(ICollectionObservable<T> collection)
        {
            this.collection = collection;
        }

        public IDisposable Subscribe(IObserver<IValueEventArgs<int>> observer)
            => new Instance(this, collection, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collectionStream;
            private IObserver<IValueEventArgs<int>> _observer;
            private ValueEventArgs<int> _args = new ValueEventArgs<int>();
            private int _count;
            private bool _disposed = false;

            public Instance(IObservable source, ICollectionObservable<T> collection, IObserver<IValueEventArgs<int>> observer)
            {
                _observer = observer;
                _args.source = source;
                _collectionStream = collection.Subscribe(
                    HandleSourceChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );
            }

            private void HandleSourceChanged(ICollectionEventArgs<T> args)
            {
                switch (args.operationType)
                {
                    case OpType.Add:

                        _args.previousValue = _count;
                        _count++;
                        _args.currentValue = _count;

                        break;

                    case OpType.Remove:

                        _args.previousValue = _count;
                        _count--;
                        _args.currentValue = _count;

                        break;
                }

                _observer.OnNext(_args);
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

                _collectionStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}