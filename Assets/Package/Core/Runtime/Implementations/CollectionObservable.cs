using System;
using System.Collections;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CollectionObservable<T> : ICollectionObservable<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();

        public int count => _collection.Count;

        private List<T> _collection = new List<T>();
        private List<uint> _ids = new List<uint>();
        private uint _nextId = 0;

        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();

        private bool _executingSafeEnumerate = false;
        private bool _disposed;

        private class ObserverData : IDisposable
        {
            public ICollectionObserver<T> observer;
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

        public CollectionObservable(params T[] source) : this((IEnumerable<T>)source) { }

        public CollectionObservable(IEnumerable<T> source) : this()
        {
            _collection.AddRange(source);
        }

        public CollectionObservable() { }

        private IEnumerable<ICollectionObserver<T>> SafeObserverEnumeration()
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

        public void Add(T element)
        {
            uint id = _nextId;
            _nextId++;

            _collection.Add(element);
            _ids.Add(id);

            foreach (var observer in SafeObserverEnumeration())
                observer.OnAdd(id, element);
        }

        public bool Remove(T element)
        {
            var index = _collection.IndexOf(element);

            if (index == -1)
                return false;

            var id = _ids[index];

            _collection.RemoveAt(index);
            _ids.RemoveAt(index);

            foreach (var observer in SafeObserverEnumeration())
                observer.OnRemove(id, element);

            return true;
        }

        public void Clear()
        {
            var collection = _collection.ToArray();
            var ids = _ids.ToArray();

            _collection.Clear();
            _ids.Clear();

            for (int i = 0; i < collection.Length; i++)
            {
                var id = ids[i];
                var element = collection[i];

                foreach (var observer in SafeObserverEnumeration())
                    observer.OnRemove(id, element);
            }
        }

        public bool Contains(T element)
            => _collection.Contains(element);

        public IDisposable Subscribe(ICollectionObserver<T> observer)
        {
            var data = new ObserverData() { observer = observer, onDispose = HandleObserverDisposed };
            _observers.Add(data);

            for (int i = 0; i < _collection.Count; i++)
                data.observer.OnAdd(_ids[i], _collection[i]);

            return data;
        }

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new CollectionObserver<T>(
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

            foreach (var data in _observers)
                data.Dispose();

            _observers.Clear();
        }
    }
}