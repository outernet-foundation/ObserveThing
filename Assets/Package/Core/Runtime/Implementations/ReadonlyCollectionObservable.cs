using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ReadonlyCollectionObservable<T> : ObservableBase<ICollectionObserver<T>>, ICollectionObservable<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _collection.Select(x => x.value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _collection.Select(x => x.value).GetEnumerator();

        public int count => _collection.Count;

        private List<(uint id, T value)> _collection = new List<(uint id, T value)>();

        public ReadonlyCollectionObservable(params T[] source) : this((IEnumerable<T>)source) { }
        public ReadonlyCollectionObservable(SynchronizationContext context, params T[] source) : this(source, context) { }

        public ReadonlyCollectionObservable(IEnumerable<T> source, SynchronizationContext context = default) : base(context)
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
            var subscription = AddObserver(observer);

            for (int i = 0; i < _collection.Count; i++)
            {
                var element = _collection[i];
                observer.OnAdd(element.id, element.value);
            }

            return subscription;
        }

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new CollectionObserver<T>(
                onAdd: (_, _) => observer.OnChange(),
                onRemove: (_, _) => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}