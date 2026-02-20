using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ReadonlyCollectionObservable<T> : ICollectionObservable<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _collection.Select(x => x.value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _collection.Select(x => x.value).GetEnumerator();

        public int count => _collection.Count;

        private List<(uint id, T value)> _collection = new List<(uint id, T value)>();
        private List<ICollectionObserver<T>> _observers;
        private bool _disposed;

        public ReadonlyCollectionObservable(params T[] source) : this((IEnumerable<T>)source) { }

        public ReadonlyCollectionObservable(IEnumerable<T> source)
        {
            uint nextId = 0;
            foreach (var element in source)
            {
                _collection.Add(new(nextId, element));
                nextId++;
            }
        }

        public bool Contains(T element)
            => _collection.Select(x => x.value).Contains(element);

        public IDisposable Subscribe(ICollectionObserver<T> observer)
        {
            _observers.Add(observer);

            for (int i = 0; i < _collection.Count; i++)
            {
                var element = _collection[i];
                observer.OnAdd(element.id, element.value);
            }

            return new Disposable(() => _observers.Remove(observer));
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

            foreach (var observer in _observers)
                observer.OnDispose();

            _observers.Clear();
        }
    }
}