using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ContainsCollectionObservableReactive<T> : IValueObservable<bool>
    {
        private ICollectionObservable<T> _collection;
        private IValueObservable<T> _contains;

        public ContainsCollectionObservableReactive(ICollectionObservable<T> collection, IValueObservable<T> contains)
        {
            _collection = collection;
            _contains = contains;
        }

        public IDisposable Subscribe(IObserver<ValueEventArgs<bool>> observer)
            => new Instance(this, _collection, _contains, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collectionStream;
            private IDisposable _containsStream;
            private IObserver<ValueEventArgs<bool>> _observer;
            private ValueEventArgs<bool> _args = new ValueEventArgs<bool>();
            private bool _disposed = false;

            private T _contains;
            private int _count;
            private List<T> _collection = new List<T>();

            public Instance(IObservable source, ICollectionObservable<T> collection, IValueObservable<T> contains, IObserver<ValueEventArgs<bool>> observer)
            {
                _observer = observer;
                _args.source = source;
                _containsStream = contains.Subscribe(HandleContainsSourceChanged, HandleSourceError, HandleSourceDisposed);
                _collectionStream = collection.Subscribe(HandleCollectionSourceChanged, HandleSourceError, HandleSourceDisposed);
            }

            private void HandleContainsSourceChanged(ValueEventArgs<T> args)
            {
                bool didContain = _count > 0;

                _contains = args.currentValue;
                _count = _collection.Count(x => Equals(x, _contains));

                bool currentlyContains = _count > 0;

                if (didContain == currentlyContains)
                    return;

                _args.previousValue = didContain;
                _args.currentValue = currentlyContains;
                _observer.OnNext(_args);
            }

            private void HandleCollectionSourceChanged(CollectionEventArgs<T> args)
            {
                switch (args.operationType)
                {
                    case OpType.Add:

                        if (Equals(args.element, _contains))
                        {
                            _count++;
                            if (_count == 1)
                            {
                                _args.currentValue = true;
                                _args.previousValue = false;
                                _observer.OnNext(_args);
                            }
                        }

                        break;

                    case OpType.Remove:

                        _count--;
                        if (_count == 0)
                        {
                            _args.currentValue = false;
                            _args.previousValue = true;
                            _observer.OnNext(_args);
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

                _containsStream.Dispose();
                _collectionStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}