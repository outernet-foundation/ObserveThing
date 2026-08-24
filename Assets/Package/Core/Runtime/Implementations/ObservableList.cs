using System;
using System.Collections;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IListOp : IOperation
    {
        uint elementId { get; }
        int index { get; }
        object element { get; }
        bool isRemove { get; }
    }

    public struct ListOp<T> : IListOp
    {
        public IObservable<IOperation> source { get; set; }
        public int index { get; set; }
        public uint elementId { get; set; }
        public T element { get; set; }
        public bool isRemove { get; set; }

        object IListOp.element => element;
    }

    public class ObservableList<T> : ObservableBase<IListObserver<T>, ListOp<T>>, IListObservable<T>, IEnumerable<T>
    {
        public T this[int index]
        {
            get => _list[index];
            set
            {
                RemoveAt(index);
                Insert(index, value);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _list.GetEnumerator();

        public int count => _list.Count;

        private List<T> _list = new List<T>();
        private List<uint> _ids = new List<uint>();
        private CollectionIdProvider _idProvider;

        public ObservableList() : this(default, default(IEnumerable<T>)) { }
        public ObservableList(params T[] value) : this(default, (IEnumerable<T>)value) { }
        public ObservableList(IEnumerable<T> value) : this(default, value) { }

        public ObservableList(ObservationContext context) : this(context, default(IEnumerable<T>)) { }
        public ObservableList(ObservationContext context, params T[] value) : this(context, (IEnumerable<T>)value) { }
        public ObservableList(ObservationContext context, IEnumerable<T> value) : base(context)
        {
            _idProvider = new CollectionIdProvider(_ids.Contains);

            if (value == null)
                return;

            foreach (var element in value)
            {
                _list.Add(element);
                _ids.Add(_idProvider.GetUnusedId());
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
            var index = _list.IndexOf(removed);

            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            var removed = _list[index];
            var id = _ids[index];
            _list.RemoveAt(index);
            _ids.RemoveAt(index);
            EnqueuePendingOperation(new ListOp<T>() { source = this, element = removed, elementId = id, index = index, isRemove = true });
        }

        public void Insert(int index, T item)
        {
            var id = _idProvider.GetUnusedId();
            _list.Insert(index, item);
            _ids.Insert(index, id);
            EnqueuePendingOperation(new ListOp<T>() { source = this, element = item, elementId = id, index = index, isRemove = false });
        }

        public void Clear()
        {
            while (_list.Count > 0)
                RemoveAt(_list.Count - 1);
        }

        public int IndexOf(T item)
            => _list.IndexOf(item);

        public bool Contains(T item)
            => _list.Contains(item);

        protected override IEnumerable<ListOp<T>> GetInitializationOperations()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                yield return new ListOp<T>() { source = this, element = _list[i], elementId = _ids[i], index = i, isRemove = false };
            }
        }

        protected override void SendOperation(IListObserver<T> observer, ListOp<T> operation)
        {
            if (operation.isRemove)
            {
                observer.OnRemove(operation.elementId, operation.index, operation.element);
            }
            else
            {
                observer.OnAdd(operation.elementId, operation.index, operation.element);
            }
        }

        IDisposable IListObservable.Subscribe(IListObserver observer, bool immediate, uint? priority)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, index, element) => observer.OnAdd(id, index, element),
                onRemove: (id, index, element) => observer.OnRemove(id, index, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable<T>.Subscribe(ICollectionObserver<T> observer, bool immediate, uint? priority)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, index, element) => observer.OnAdd(id, element),
                onRemove: (id, index, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, index, element) => observer.OnAdd(id, element),
                onRemove: (id, index, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}