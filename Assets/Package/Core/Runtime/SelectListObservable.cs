using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class SelectListObservable<T, U> : IListObservable<U>
    {
        public IListObservable<T> list;
        public Func<T, U> select;

        public SelectListObservable(IListObservable<T> list, Func<T, U> select)
        {
            this.list = list;
            this.select = select;
        }

        public IDisposable Subscribe(IObserver<IListEventArgs<U>> observer)
            => new Instance(this, list, select, observer);

        private class Instance : IDisposable
        {
            private IDisposable _listStream;
            private Func<T, U> _select;
            private IObserver<IListEventArgs<U>> _observer;
            private ListEventArgs<U> _args = new ListEventArgs<U>();
            private bool _disposed = false;

            private List<U> _currentList = new List<U>();

            public Instance(IObservable source, IListObservable<T> list, Func<T, U> select, IObserver<IListEventArgs<U>> observer)
            {
                _select = select;
                _observer = observer;
                _args.source = source;
                _listStream = list.Subscribe(HandleSourceChanged, HandleSourceError, HandleSourceDisposed);
            }

            private void HandleSourceChanged(IListEventArgs<T> args)
            {
                _args.operationType = args.operationType;

                switch (args.operationType)
                {
                    case OpType.Add:

                        var added = _select(args.element);
                        _currentList.Insert(args.index, added);

                        _args.operationType = OpType.Add;
                        _args.element = added;
                        _args.index = args.index;

                        _observer.OnNext(_args);

                        break;

                    case OpType.Remove:

                        var removed = _currentList[args.index];
                        _currentList.RemoveAt(args.index);

                        _args.operationType = OpType.Remove;
                        _args.element = removed;
                        _args.index = args.index;

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

                _listStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}