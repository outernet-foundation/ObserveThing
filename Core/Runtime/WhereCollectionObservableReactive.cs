using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class WhereCollectionObservableReactive<T> : ICollectionObservable<T>
    {
        public ICollectionObservable<T> collection;
        public Func<T, IValueObservable<bool>> select;

        public WhereCollectionObservableReactive(ICollectionObservable<T> collection, Func<T, IValueObservable<bool>> select)
        {
            this.collection = collection;
            this.select = select;
        }

        public IDisposable Subscribe(IObserver<CollectionEventArgs<T>> observer)
            => new Instance(this, collection, select, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collectionStream;
            private Func<T, IValueObservable<bool>> _select;
            private IObserver<CollectionEventArgs<T>> _observer;
            private CollectionEventArgs<T> _args = new CollectionEventArgs<T>();
            private bool _disposed = false;

            private class FilterData
            {
                public T element;
                public int count;
                public IDisposable filter;
                public bool included;
            }

            private Dictionary<T, FilterData> _filterData = new Dictionary<T, FilterData>();

            public Instance(IObservable source, ICollectionObservable<T> collection, Func<T, IValueObservable<bool>> select, IObserver<CollectionEventArgs<T>> observer)
            {
                _select = select;
                _observer = observer;
                _args.source = source;
                _collectionStream = collection.Subscribe(
                    HandleSourceChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );
            }

            private void HandleSourceError(Exception error)
            {
                _observer.OnError(error);
            }

            private void HandleSourceDisposed()
            {
                Dispose();
            }

            private void HandleSourceChanged(CollectionEventArgs<T> args)
            {
                switch (args.operationType)
                {
                    case OpType.Add:

                        if (!_filterData.TryGetValue(args.element, out var added))
                        {
                            added = new FilterData() { element = args.element };
                            _filterData.Add(args.element, added);
                            added.filter = _select(args.element).Subscribe(x => HandleFilterChanged(x.currentValue, x.previousValue, added));
                        }

                        added.count++;

                        if (added.included)
                        {
                            _args.operationType = args.operationType;
                            _args.element = args.element;
                            _observer.OnNext(_args);
                        }

                        break;

                    case OpType.Remove:

                        var removed = _filterData[args.element];
                        removed.count--;

                        if (removed.included)
                        {
                            _args.operationType = args.operationType;
                            _args.element = args.element;
                            _observer.OnNext(_args);
                        }

                        if (removed.count == 0)
                        {
                            removed.filter.Dispose();
                            _filterData.Remove(args.element);
                        }

                        break;
                }
            }

            private void HandleFilterChanged(bool included, bool wasIncluded, FilterData filterData)
            {
                if (included == wasIncluded)
                    return;

                filterData.included = included;
                _args.element = filterData.element;
                _args.operationType = included ? OpType.Add : OpType.Remove;

                for (int i = 0; i < filterData.count; i++)
                    _observer.OnNext(_args);
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                foreach (var data in _filterData.Values)
                    data.filter.Dispose();

                _collectionStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}