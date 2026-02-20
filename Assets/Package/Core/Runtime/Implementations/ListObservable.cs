using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ListObservable<T> : IListObservable<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _list.Select(x => x.value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.Select(x => x.value).GetEnumerator();

        public int count => _list.Count;
        public T this[int index]
        {
            get => _list[index].value;
            set
            {
                if (Equals(_list[index].value, value))
                    return;

                RemoveAt(index);
                Insert(index, value);
            }
        }

        private List<(uint id, T value)> _list = new List<(uint id, T value)>();
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();
        private bool _notifyingObservers;
        private bool _disposed;

        private CollectionIdProvider _idProvider;

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

        public ListObservable(params T[] source) : this((IEnumerable<T>)source) { }

        public ListObservable(IEnumerable<T> source) : this()
        {
            foreach (var element in source)
                _list.Add(new(_idProvider.GetUnusedId(), element));
        }

        public ListObservable()
        {
            _idProvider = new CollectionIdProvider(x => _list.Any(item => item.id == x));
        }

        private void NotifyObservers(Action<IListObserver<T>> notify)
        {
            if (_notifyingObservers)
                throw new Exception("Cannot notify observers while already notifying observers.");

            _notifyingObservers = true;

            int count = _observers.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _observers[i];

                if (instance.disposed)
                    continue;

                try
                {
                    notify(instance.observer);
                }
                catch (Exception exc)
                {
                    // TODO: Decide
                    // Should errors thrown by observers be looped back into the observer's onError callback?
                    instance.observer.OnError(exc);
                }
            }

            _notifyingObservers = false;

            foreach (var disposed in _disposedObservers)
                _observers.Remove(disposed);
        }

        private void HandleObserverDisposed(ObserverData observer)
        {
            if (_disposed)
                return;

            if (_notifyingObservers)
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
            var index = _list.FindIndex(x => Equals(x.value, removed));

            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            var removed = _list[index];
            _list.RemoveAt(index);
            NotifyObservers(x => x.OnRemove(removed.id, index, removed.value));
        }

        public void Insert(int index, T inserted)
        {
            var id = _idProvider.GetUnusedId();
            _list.Insert(index, new(id, inserted));
            NotifyObservers(x => x.OnAdd(id, index, inserted));
        }

        public void Clear()
        {
            while (_list.Count > 0)
                RemoveAt(_list.Count - 1);
        }

        public int IndexOf(T item)
            => _list.FindIndex(x => Equals(x.value, item));

        public bool Contains(T item)
            => _list.Any(x => Equals(x.value, item));

        public IDisposable Subscribe(IListObserver<T> observer)
        {
            var data = new ObserverData() { observer = observer, onDispose = HandleObserverDisposed };

            _observers.Add(data);

            for (int i = 0; i < _list.Count; i++)
            {
                var element = _list[i];
                data.observer.OnAdd(element.id, i, element.value);
            }

            return data;
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, _, value) => observer.OnAdd(id, value),
                onRemove: (id, _, value) => observer.OnRemove(id, value),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (_, _, _) => observer.OnChange(),
                onRemove: (_, _, _) => observer.OnChange(),
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