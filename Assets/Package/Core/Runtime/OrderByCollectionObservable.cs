using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class OrderByCollectionObservable<T, U> : IListObservable<T>
    {
        public ICollectionObservable<T> collection;
        public Func<T, U> orderBy;

        public OrderByCollectionObservable(ICollectionObservable<T> collection, Func<T, U> orderBy)
        {
            this.collection = collection;
            this.orderBy = orderBy;
        }

        public IDisposable Subscribe(IObserver<IListEventArgs<T>> observer)
            => new Instance(this, collection, orderBy, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collectionStream;
            private Func<T, U> _orderBy;
            private IObserver<ListEventArgs<T>> _observer;
            private ListEventArgs<T> _args = new ListEventArgs<T>();
            private bool _disposed = false;

            private List<(T value, U orderedBy)> _elements = new List<(T value, U orderedBy)>();

            public Instance(IObservable source, ICollectionObservable<T> collection, Func<T, U> orderBy, IObserver<ListEventArgs<T>> observer)
            {
                _orderBy = orderBy;
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
                        {
                            U orderByElement = _orderBy(args.element);
                            int? sortIndex = default;

                            for (int i = 0; i < _elements.Count; i++)
                            {
                                if (Comparer<U>.Default.Compare(orderByElement, _elements[i].orderedBy) <= 0)
                                {
                                    sortIndex = i;
                                    break;
                                }
                            }

                            var index = sortIndex ?? _elements.Count;
                            _elements.Insert(index, new(args.element, orderByElement));

                            _args.operationType = OpType.Add;
                            _args.element = args.element;
                            _args.index = index;

                            _observer.OnNext(_args);
                        }

                        break;

                    case OpType.Remove:
                        {
                            for (int i = 0; i < _elements.Count; i++)
                            {
                                if (Equals(_elements[i].value, args.element))
                                {
                                    _elements.RemoveAt(i);

                                    _args.operationType = OpType.Remove;
                                    _args.element = args.element;
                                    _args.index = i;

                                    _observer.OnNext(_args);

                                    return;
                                }
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

                _collectionStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}