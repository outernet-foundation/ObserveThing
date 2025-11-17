using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ShallowCopyListObservable<T> : IListObservable<T>
    {
        public IListObservable<IValueObservable<T>> list;

        public ShallowCopyListObservable(IListObservable<IValueObservable<T>> list)
        {
            this.list = list;
        }

        public IDisposable Subscribe(IObserver<IListEventArgs<T>> observer)
            => new Instance(this, list, observer);

        private class Instance : IDisposable
        {
            private IDisposable _listStream;
            private IObserver<IListEventArgs<T>> _observer;
            private ListEventArgs<T> _args = new ListEventArgs<T>();
            private bool _disposed = false;

            private List<IValueObservable<T>> _currentList = new List<IValueObservable<T>>();

            private class ElementData
            {
                public IValueObservable<T> element;
                public T value;
                public IDisposable valueStream;
                public bool initialized;
                public bool disposed;

                public void Dispose()
                {
                    disposed = true;
                    valueStream.Dispose();
                }
            }

            private Dictionary<IValueObservable<T>, ElementData> _elementData = new Dictionary<IValueObservable<T>, ElementData>();

            public Instance(IObservable source, IListObservable<IValueObservable<T>> list, IObserver<IListEventArgs<T>> observer)
            {
                _observer = observer;
                _args.source = source;
                _listStream = list.Subscribe(
                    HandleSourceChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );
            }

            private void HandleSourceChanged(IListEventArgs<IValueObservable<T>> args)
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

                        _currentList.Insert(args.index, added.element);

                        _args.operationType = OpType.Add;
                        _args.element = added.value;
                        _args.index = args.index;

                        _observer.OnNext(_args);

                        break;

                    case OpType.Remove:
                        var removed = _elementData[args.element];

                        _currentList.Remove(removed.element);

                        if (!_currentList.Contains(removed.element))
                        {
                            removed.Dispose();
                            _elementData.Remove(args.element);
                        }

                        _args.operationType = OpType.Remove;
                        _args.element = removed.value;
                        _args.index = args.index;

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

                for (int i = 0; i < _currentList.Count; i++)
                {
                    if (_currentList[i] == elementData.element)
                    {
                        _args.element = previousValue;
                        _args.operationType = OpType.Remove;
                        _args.index = i;

                        _observer.OnNext(_args);

                        _args.element = value;
                        _args.operationType = OpType.Add;
                        _args.index = i;

                        _observer.OnNext(_args);
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                foreach (var data in _elementData.Values)
                    data.Dispose();

                _listStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}