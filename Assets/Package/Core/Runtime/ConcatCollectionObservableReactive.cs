using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ConcatCollectionObservableReactive<T> : ICollectionObservable<T>
    {
        public ICollectionObservable<T> collection1;
        public ICollectionObservable<T> collection2;

        public ConcatCollectionObservableReactive(ICollectionObservable<T> collection1, ICollectionObservable<T> collection2)
        {
            this.collection1 = collection1;
            this.collection2 = collection2;
        }

        public IDisposable Subscribe(IObserver<ICollectionEventArgs<T>> observer)
            => new Instance(this, collection1, collection2, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collection1Stream;
            private IDisposable _collection2Stream;
            private IObserver<ICollectionEventArgs<T>> _observer;
            private CollectionEventArgs<T> _args = new CollectionEventArgs<T>();
            private bool _disposed = false;

            public Instance(IObservable source, ICollectionObservable<T> collection1, ICollectionObservable<T> collection2, IObserver<ICollectionEventArgs<T>> observer)
            {
                _observer = observer;
                _args.source = source;

                _collection1Stream = collection1.Subscribe(
                    HandleSourceChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );

                _collection2Stream = collection2.Subscribe(
                    HandleSourceChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );
            }

            private void HandleSourceChanged(ICollectionEventArgs<T> args)
            {
                _args.operationType = args.operationType;

                switch (args.operationType)
                {
                    case OpType.Add:

                        _args.element = args.element;
                        _observer.OnNext(_args);

                        break;

                    case OpType.Remove:

                        _args.element = args.element;
                        _observer.OnNext(_args);

                        break;
                }
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

                _collection1Stream.Dispose();
                _collection2Stream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}