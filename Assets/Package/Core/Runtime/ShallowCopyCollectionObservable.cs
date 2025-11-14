using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ObserveThing
{
    public class ShallowCopyCollectionObservable<T> : ICollectionObservable<T>
    {
        public ICollectionObservable<IValueObservable<T>> collection;

        public ShallowCopyCollectionObservable(ICollectionObservable<IValueObservable<T>> collection)
        {
            this.collection = collection;
        }

        public IDisposable Subscribe(IObserver<ICollectionEventArgs<T>> observer)
            => new Instance(this, collection, observer);

        private class Instance : IDisposable
        {
            private IDisposable _collectionStream;
            private IObserver<ICollectionEventArgs<T>> _observer;
            private CollectionEventArgs<T> _args = new CollectionEventArgs<T>();
            private bool _disposed = false;

            private class ElementData
            {
                public IValueObservable<T> element;
                public bool initialized;
                public bool disposed;
                public T value;
                public int count;
                public IDisposable valueStream;

                public void Dispose()
                {
                    disposed = true;
                    valueStream.Dispose();
                }
            }

            private Dictionary<IValueObservable<T>, ElementData> _elementData = new Dictionary<IValueObservable<T>, ElementData>();

            public Instance(IObservable source, ICollectionObservable<IValueObservable<T>> collection, IObserver<ICollectionEventArgs<T>> observer)
            {
                _observer = observer;
                _args.source = source;
                _collectionStream = collection.Subscribe(
                    HandleSourceChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );
            }

            private void HandleSourceChanged(ICollectionEventArgs<IValueObservable<T>> args)
            {
                switch (args.operationType)
                {
                    case OpType.Add:
                        if (!_elementData.TryGetValue(args.element, out var added))
                        {
                            added = new ElementData() { element = args.element };
                            _elementData.Add(args.element, added);

                            added.valueStream = args.element.Subscribe(
                                x => HandleSelectedChanged(x.currentValue, added),
                                HandleSourceError,
                                () => HandleSelectedDisposed(added)
                            );

                            added.initialized = true;
                        }

                        added.count++;

                        _args.operationType = OpType.Add;
                        _args.element = added.value;
                        _observer.OnNext(_args);

                        break;

                    case OpType.Remove:
                        var removed = _elementData[args.element];
                        var removedCount = removed.count;

                        removed.count--;

                        if (removed.count == 0)
                        {
                            removed.Dispose();
                            _elementData.Remove(args.element);
                        }

                        _args.operationType = OpType.Remove;
                        _args.element = removed.value;

                        for (int i = 0; i < removedCount; i++)
                            _observer.OnNext(_args);

                        break;
                }
            }

            private void HandleSelectedDisposed(ElementData elementData)
            {
                if (elementData.disposed)
                    return;

                HandleSourceError(new Exception("Source element disposed unexpectedly."));
            }

            private void HandleSourceError(Exception error)
            {
                _observer.OnError(error);
            }

            private void HandleSourceDisposed()
            {
                Dispose();
            }

            private void HandleSelectedChanged(T value, ElementData elementData)
            {
                if (!elementData.initialized)
                {
                    elementData.value = value;
                    elementData.initialized = true;
                    return;
                }

                T previousValue = elementData.value;
                elementData.value = value;

                for (int i = 0; i < elementData.count; i++)
                {
                    _args.element = previousValue;
                    _args.operationType = OpType.Remove;

                    _observer.OnNext(_args);

                    _args.element = value;
                    _args.operationType = OpType.Add;

                    _observer.OnNext(_args);
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                foreach (var data in _elementData.Values)
                    data.Dispose();

                _collectionStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}