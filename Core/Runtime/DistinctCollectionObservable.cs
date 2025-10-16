using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class DistinctCollectionObservable<T> : ICollectionObservable<T>
    {
        public ICollectionObservable<T> collection;

        public DistinctCollectionObservable(ICollectionObservable<T> collection)
        {
            this.collection = collection;
        }

        public IDisposable Subscribe(IObserver<CollectionEventArgs<T>> observer)
            => new Instance(this, collection, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collection;
            private IObserver<CollectionEventArgs<T>> _observer;
            private CollectionEventArgs<T> _args = new CollectionEventArgs<T>();
            private bool _disposed = false;

            private Dictionary<T, int> _elements = new Dictionary<T, int>();

            public Instance(IObservable source, ICollectionObservable<T> collection, IObserver<CollectionEventArgs<T>> observer)
            {
                _observer = observer;
                _args.source = source;
                _collection = collection.Subscribe(
                    HandleSourceChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );
            }

            private void HandleSourceChanged(CollectionEventArgs<T> args)
            {
                switch (args.operationType)
                {
                    case OpType.Add:
                        {

                            _elements.TryGetValue(args.element, out var count);

                            count++;
                            _elements[args.element] = count;

                            if (count == 1)
                            {
                                _args.operationType = OpType.Add;
                                _args.element = args.element;
                                _observer.OnNext(_args);
                            }
                        }

                        break;

                    case OpType.Remove:
                        {
                            int count = _elements[args.element];

                            count--;
                            _elements[args.element] = count;

                            if (count == 0)
                            {
                                _elements.Remove(args.element);
                                _args.operationType = OpType.Remove;
                                _args.element = args.element;
                                _observer.OnNext(_args);
                            }
                        }

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

                _collection.Dispose();
                _observer.OnDispose();
            }
        }
    }
}