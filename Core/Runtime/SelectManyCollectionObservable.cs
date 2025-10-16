using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class SelectManyCollectionObservable<T, U> : ICollectionObservable<U>
    {
        public ICollectionObservable<T> collection;
        public Func<T, IEnumerable<U>> selectMany;

        public SelectManyCollectionObservable(ICollectionObservable<T> collection, Func<T, IEnumerable<U>> selectMany)
        {
            this.collection = collection;
            this.selectMany = selectMany;
        }

        public IDisposable Subscribe(IObserver<CollectionEventArgs<U>> observer)
            => new Instance(this, collection, selectMany, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collectionStream;
            private Func<T, IEnumerable<U>> _selectMany;
            private IObserver<CollectionEventArgs<U>> _observer;
            private CollectionEventArgs<U> _args = new CollectionEventArgs<U>();
            private bool _disposed = false;

            private Dictionary<T, SelectManyData> _selectData = new Dictionary<T, SelectManyData>();
            private class SelectManyData
            {
                public T value;
                public int count;
                public U[] addedElements;
            }

            public Instance(IObservable source, ICollectionObservable<T> collection, Func<T, IEnumerable<U>> selectMany, IObserver<CollectionEventArgs<U>> observer)
            {
                _observer = observer;
                _selectMany = selectMany;
                _args.source = source;
                _collectionStream = collection.Subscribe(
                    HandleSourceChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );
            }

            private void HandleSourceChanged(CollectionEventArgs<T> args)
            {
                _args.operationType = args.operationType;

                switch (args.operationType)
                {
                    case OpType.Add:

                        if (!_selectData.TryGetValue(args.element, out var added))
                        {
                            added = new SelectManyData();
                            added.value = args.element;
                            added.addedElements = _selectMany(args.element).ToArray();
                            _selectData.Add(args.element, added);
                        }

                        added.count++;

                        foreach (var element in added.addedElements)
                        {
                            _args.element = element;
                            _observer.OnNext(_args);
                        }

                        break;

                    case OpType.Remove:

                        var removed = _selectData[args.element];

                        foreach (var element in removed.addedElements)
                        {
                            _args.element = element;
                            _observer.OnNext(_args);
                        }

                        removed.count--;

                        if (removed.count == 0)
                            _selectData.Remove(args.element);

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