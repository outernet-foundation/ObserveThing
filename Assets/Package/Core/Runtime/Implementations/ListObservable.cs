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
        public T this[int index]
        {
            get => _list[index];
            set
            {
                if (Equals(_list[index], value))
                    return;

                RemoveAt(index);
                Insert(index, value);
            }
        }

        private List<T> _list = new List<T>();
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _executingSafeEnumerate;
        private bool _disposed;

        private class ObserverData : IDisposable
        {
            public IListObserver<T> observer;
            public Action<ObserverData> onDispose;
            public bool disposed { get; private set; }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                onDispose?.Invoke(this);
                observer.OnDispose();
            }
        }

        public ListObservable(params T[] source) : this((IEnumerable<T>)source)
        { }

        public ListObservable(IEnumerable<T> source) : this()
        {
            _list.AddRange(source);
        }

        public ListObservable() { }

        private IEnumerable<IListObserver<T>> SafeObserverEnumeration()
        {
            if (_executingSafeEnumerate)
                throw new Exception("Cannot apply changes while already applying changes");

            _executingSafeEnumerate = true;

            int count = _observers.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _observers[i];
                if (instance.disposed)
                    continue;

                yield return instance.observer;
            }

            _executingSafeEnumerate = true;

            foreach (var disposed in _disposedObservers)
                _observers.Remove(disposed);
        }

        private void HandleObserverDisposed(ObserverData observer)
        {
            if (_disposed)
                return;

            if (_executingSafeEnumerate)
            {
                _disposedObservers.Add(observer);
                return;
            }

            _observers.Remove(observer);
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

            foreach (var observer in SafeObserverEnumeration())
                observer.OnRemove(index, removed);
        }

        public void Insert(int index, T inserted)
        {
            _list.Insert(index, inserted);

            foreach (var observer in SafeObserverEnumeration())
                observer.OnAdd(index, inserted);
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

        public IDisposable Subscribe(IListObserver<T> observer)
        {
            var data = new ObserverData() { observer = observer, onDispose = HandleObserverDisposed };

            _observers.Add(data);

            for (int i = 0; i < _list.Count; i++)
                data.observer.OnAdd(i, _list[i]);

            return data;
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (_, value) => observer.OnAdd(value),
                onRemove: (_, value) => observer.OnRemove(value),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (_, _) => observer.OnChange(),
                onRemove: (_, _) => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var instance in _observers)
                instance.Dispose();

            _observers.Clear();
        }
    }
}