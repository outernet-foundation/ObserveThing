using System;
using System.Collections;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ListObservable<T> : IListObservable<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public int count => _list.Count;
        public T this[int index] => _list[index];

        private List<T> _list = new List<T>();
        private ListEventArgs<T> _args = new ListEventArgs<T>();
        private List<Instance> _instances = new List<Instance>();
        private List<Instance> _disposedInstances = new List<Instance>();
        private bool _executingOnNext;
        private bool _disposed;
        private IDisposable _fromSubscription;

        public ListObservable()
        {
            _args.source = this;
        }

        private void SafeOnNext(ListEventArgs<T> args)
        {
            _executingOnNext = true;

            int count = _instances.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _instances[i];
                if (instance.disposed)
                    continue;

                instance.OnNext(args);
            }

            foreach (var disposedInstance in _disposedInstances)
                _instances.Remove(disposedInstance);

            _executingOnNext = false;
        }

        public void Add(T added)
            => Insert(_list.Count, added);

        public void AddRange(IEnumerable<T> toAdd)
        {
            foreach (var added in toAdd)
                Add(added);
        }

        public void Remove(T removed)
            => RemoveAt(_list.IndexOf(removed));

        public void RemoveAt(int index)
        {
            T element = _list[index];
            _list.RemoveAt(index);
            _args.index = index;
            _args.element = element;
            _args.operationType = OpType.Remove;

            SafeOnNext(_args);
        }

        public void Insert(int index, T inserted)
        {
            _list.Insert(index, inserted);
            _args.index = index;
            _args.element = inserted;
            _args.operationType = OpType.Add;

            SafeOnNext(_args);
        }

        public void Clear()
        {
            while (_list.Count > 0)
                RemoveAt(_list.Count - 1);
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void From(IEnumerable<T> source)
        {
            _fromSubscription?.Dispose();
            Clear();

            foreach (var element in source)
                Add(element);
        }

        public void From(IListObservable<T> source)
        {
            _fromSubscription?.Dispose();
            _fromSubscription = source.Subscribe(
                x =>
                {
                    if (x.operationType == OpType.Add)
                        Insert(x.index, x.element);
                    else if (x.operationType == OpType.Remove)
                        RemoveAt(x.index);
                }
            );
        }

        public IDisposable Subscribe(IObserver<ListEventArgs<T>> observer)
        {
            var instance = new Instance(observer, x =>
            {
                if (_disposed)
                    return;

                if (_executingOnNext)
                {
                    _disposedInstances.Add(x);
                    return;
                }

                _instances.Remove(x);
            });

            _instances.Add(instance);

            for (int i = 0; i < _list.Count; i++)
            {
                _args.index = i;
                _args.element = _list[i];
                _args.operationType = OpType.Add;
                instance.OnNext(_args);
            }

            return instance;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var instance in _instances)
                instance.Dispose();

            _instances.Clear();
            _fromSubscription?.Dispose();
        }

        private class Instance : IDisposable
        {
            public bool disposed { get; private set; }

            private IObserver<ListEventArgs<T>> _observer;
            private Action<Instance> _onDispose;

            public Instance(IObserver<ListEventArgs<T>> observer, Action<Instance> onDispose)
            {
                _observer = observer;
                _onDispose = onDispose;
            }

            public void OnNext(ListEventArgs<T> args)
            {
                _observer?.OnNext(args);
            }

            public void OnError(Exception error)
            {
                _observer?.OnError(error);
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                _observer.OnDispose();
                _observer = null;

                _onDispose(this);
            }
        }
    }
}