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
            _collection.Add(element);

            foreach (var observer in SafeObserverEnumeration())
                observer.OnAdd(element);
        }

        public bool Remove(T element)
        {
            if (!_collection.Remove(element))
                return false;

            foreach (var observer in SafeObserverEnumeration())
                observer.OnRemove(element);

            return true;
        }

        public void Clear()
        {
            foreach (var element in _collection.ToArray())
            {
                _collection.Remove(element);

                foreach (var observer in SafeObserverEnumeration())
                    observer.OnRemove(element);
            }
        }

        public bool Contains(T element)
            => _collection.Contains(element);

        public IDisposable Subscribe(ICollectionObserver<T> observer)
        {
            var data = new ObserverData() { observer = observer, onDispose = HandleObserverDisposed };
            _observers.Add(data);

            foreach (var element in this)
                data.observer.OnAdd(element);

            return data;
        }

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new CollectionObserver<T>(
                onAdd: _ => observer.OnChange(),
                onRemove: _ => observer.OnChange(),
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