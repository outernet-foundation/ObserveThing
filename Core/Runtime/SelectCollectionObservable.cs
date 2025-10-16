using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class SelectCollectionObservable<T, U> : ICollectionObservable<U>
    {
        public ICollectionObservable<T> collection;
        public Func<T, U> select;

        public SelectCollectionObservable(ICollectionObservable<T> collection, Func<T, U> select)
        {
            this.collection = collection;
            this.select = select;
        }

        public IDisposable Subscribe(IObserver<CollectionEventArgs<U>> observer)
            => new Instance(this, collection, select, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collectionStream;
            private Func<T, U> _select;
            private IObserver<CollectionEventArgs<U>> _observer;
            private CollectionEventArgs<U> _args = new CollectionEventArgs<U>();
            private bool _disposed = false;

            private Dictionary<T, SelectData> _selectedData = new Dictionary<T, SelectData>();

            private class SelectData
            {
                public T element;
                public U selected;
                public int count;
            }

            public Instance(IObservable source, ICollectionObservable<T> collection, Func<T, U> select, IObserver<CollectionEventArgs<U>> observer)
            {
                _select = select;
                _observer = observer;
                _args.source = source;
                _collectionStream = collection.Subscribe(HandleSourceChanged, HandleSourceError, HandleSourceDisposed);
            }

            private void HandleSourceChanged(CollectionEventArgs<T> args)
            {
                _args.operationType = args.operationType;

                switch (args.operationType)
                {
                    case OpType.Add:

                        if (!_selectedData.TryGetValue(args.element, out var added))
                        {
                            added = new SelectData() { element = args.element, selected = _select(args.element) };
                            _selectedData.Add(args.element, added);
                        }

                        added.count++;

                        _args.element = added.selected;
                        _observer.OnNext(_args);

                        break;

                    case OpType.Remove:

                        var removed = _selectedData[args.element];
                        removed.count--;

                        if (removed.count == 0)
                            _selectedData.Remove(args.element);

                        _args.element = removed.selected;
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

                _collectionStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}