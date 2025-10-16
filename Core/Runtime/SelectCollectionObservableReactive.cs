using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class SelectCollectionObservableReactive<T, U> : ICollectionObservable<U>
    {
        public ICollectionObservable<T> collection;
        public Func<T, IValueObservable<U>> select;

        public SelectCollectionObservableReactive(ICollectionObservable<T> collection, Func<T, IValueObservable<U>> select)
        {
            this.collection = collection;
            this.select = select;
        }

        public IDisposable Subscribe(IObserver<CollectionEventArgs<U>> observer)
            => new Instance(this, collection, select, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collectionStream;
            private Func<T, IValueObservable<U>> _select;
            private IObserver<CollectionEventArgs<U>> _observer;
            private CollectionEventArgs<U> _args = new CollectionEventArgs<U>();
            private bool _disposed = false;

            private class SelectData
            {
                public T element;
                public U selected;
                public int count;
                public IDisposable select;
            }

            private Dictionary<T, SelectData> _selectData = new Dictionary<T, SelectData>();

            public Instance(IObservable source, ICollectionObservable<T> collection, Func<T, IValueObservable<U>> select, IObserver<CollectionEventArgs<U>> observer)
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

            private void HandleSourceChanged(CollectionEventArgs<T> args)
            {
                switch (args.operationType)
                {
                    case OpType.Add:

                        if (!_selectData.TryGetValue(args.element, out var added))
                        {
                            added = new SelectData() { element = args.element };
                            _selectData.Add(args.element, added);
                            added.select = _select(args.element).Subscribe(x => HandleSelectedChanged(x.currentValue, added));
                        }

                        added.count++;

                        _args.operationType = OpType.Add;
                        _args.element = added.selected;
                        _observer.OnNext(_args);

                        break;

                    case OpType.Remove:

                        var removed = _selectData[args.element];
                        removed.count--;

                        _args.operationType = OpType.Remove;
                        _args.element = removed.selected;
                        _observer.OnNext(_args);

                        if (removed.count == 0)
                        {
                            removed.select.Dispose();
                            _selectData.Remove(args.element);
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

            private void HandleSelectedChanged(U selected, SelectData selectData)
            {
                _args.element = selectData.selected;
                _args.operationType = OpType.Remove;

                for (int i = 0; i < selectData.count; i++)
                    _observer.OnNext(_args);

                selectData.selected = selected;

                _args.element = selected;
                _args.operationType = OpType.Add;

                for (int i = 0; i < selectData.count; i++)
                    _observer.OnNext(_args);
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                foreach (var data in _selectData.Values)
                    data.select.Dispose();

                _collectionStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}