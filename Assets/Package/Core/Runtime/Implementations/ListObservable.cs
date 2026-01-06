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
        private List<Instance> _instances = new List<Instance>();
        private List<Instance> _disposedInstances = new List<Instance>();
        private bool _executingOnNext;
        private bool _disposed;
        private IDisposable _fromSubscription;

        public ListObservable(params T[] source) : this((IEnumerable<T>)source)
        { }

        public ListObservable(IEnumerable<T> source) : this()
        {
            _list.AddRange(source);
        }

        public ListObservable() { }

        private void SafeOnNext(IObservable source, T element, int index, OpType opType)
        {
            _executingOnNext = true;

            int count = _instances.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _instances[i];
                if (instance.disposed)
                    continue;

                instance.OnNext(source, element, index, opType);
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

        public bool Remove(T removed)
        {
            var index = _list.IndexOf(removed);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            T removed = _list[index];
            _list.RemoveAt(index);
            SafeOnNext(this, removed, index, OpType.Remove);
        }

        public void Insert(int index, T inserted)
        {
            _list.Insert(index, inserted);
            SafeOnNext(this, inserted, index, OpType.Add);
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

        public bool Contains(T item)
            => _list.Contains(item);

        public IDisposable Subscribe(IObserver<IListEventArgs<T>> observer)
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
                instance.OnNext(this, _list[i], i, OpType.Add);

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
            private ListEventArgs<T> _args = new ListEventArgs<T>();

            public Instance(IObserver<ListEventArgs<T>> observer, Action<Instance> onDispose)
            {
                _observer = observer;
                _onDispose = onDispose;
            }

            public void OnNext(IObservable source, T element, int index, OpType opType)
            {
                _args.source = source;
                _args.element = element;
                _args.index = index;
                _args.operationType = opType;
                _observer?.OnNext(_args);
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