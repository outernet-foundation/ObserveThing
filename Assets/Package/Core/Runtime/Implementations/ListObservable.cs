using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct ListOpData<T>
    {
        public (uint id, T element) element;
        public int index;
        public bool isRemove;
    }

    public class ListObservable<T> : IListObservable<T>, IEnumerable<T>, IDisposable
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
        private SynchronizedNotificationQueue<IListObserver<T>, ListOpData<T>> _notificationQueue;
        private CollectionIdProvider _idProvider;

        public ListObservable(params T[] source) : this(source, default) { }
        public ListObservable(SynchronizationContext context, params T[] source) : this(source, context) { }
        public ListObservable(IEnumerable<T> source, SynchronizationContext context = default) : this(context)
        {
            foreach (var element in source)
                _list.Add(new(_idProvider.GetUnusedId(), element));
        }

        public ListObservable(SynchronizationContext context = default)
        {
            _notificationQueue = new SynchronizedNotificationQueue<IListObserver<T>, ListOpData<T>>(NotifyObserver, context);
            _idProvider = new CollectionIdProvider(x => _list.Any(item => item.id == x));
        }

        private void NotifyObserver(IListObserver<T> observer, ListOpData<T> data)
        {
            if (data.isRemove)
            {
                observer.OnRemove(data.element.id, data.index, data.element.element);
            }
            else
            {
                observer.OnAdd(data.element.id, data.index, data.element.element);
            }
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
            _notificationQueue.EnqueueNotify(new ListOpData<T>() { index = index, element = removed, isRemove = true });
        }

        public void Insert(int index, T item)
        {
            (uint id, T value) inserted = new(_idProvider.GetUnusedId(), item);
            _list.Insert(index, inserted);
            _notificationQueue.EnqueueNotify(new ListOpData<T>() { index = index, element = inserted, isRemove = false });
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
            var subscription = _notificationQueue.AddObserver(observer);
            for (int i = 0; i < _list.Count; i++)
            {
                var element = _list[i];
                observer.OnAdd(element.id, i, element.value);
            }

            return subscription;
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
            _notificationQueue.Dispose();
        }
    }
}