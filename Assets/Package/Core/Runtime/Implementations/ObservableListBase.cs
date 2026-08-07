using System;
using System.Collections.Generic;
using System.Linq;

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

    public class ObservableListBase<T> : ObservableBase<IListObserver<T>, ListOp<T>>, IListObservable<T>
    {
        private List<(uint id, T value)> _list = new List<(uint id, T value)>();
        private CollectionIdProvider _idProvider;

        public ObservableListBase(ObservationContext context) : this(context, null) { }
        public ObservableListBase(ObservationContext context, IEnumerable<T> value) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _list.Any(item => item.id == x));

            if (value == null)
                return;

            foreach (var element in value)
                _list.Add(new(_idProvider.GetUnusedId(), element));
        }

        protected int GetCountInternal()
            => _list.Count;

        protected IEnumerable<(uint id, T value)> ElementsInternal()
            => _list;

        protected void AddInternal(T added)
            => InsertInternal(_list.Count, added);

        protected void AddRangeInternal(IEnumerable<T> toAdd)
        {
            foreach (var added in toAdd)
                AddInternal(added);
        }

        protected bool RemoveInternal(T removed)
        {
            var index = _list.FindIndex(x => Equals(x.value, removed));

            if (index == -1)
                return false;

            RemoveAtInternal(index);
            return true;
        }

        protected void RemoveAtInternal(int index)
        {
            var removed = _list[index];
            _list.RemoveAt(index);
            EnqueuePendingOperation(new ListOp<T>() { source = this, element = removed.value, elementId = removed.id, index = index, isRemove = true });
        }

        protected void InsertInternal(int index, T item)
        {
            (uint id, T value) inserted = new(_idProvider.GetUnusedId(), item);
            _list.Insert(index, inserted);
            EnqueuePendingOperation(new ListOp<T>() { source = this, element = inserted.value, elementId = inserted.id, index = index, isRemove = false });
        }

        protected void ClearInternal()
        {
            while (_list.Count > 0)
                RemoveAtInternal(_list.Count - 1);
        }

        protected T ElementAtInternal(int index)
            => _list[index].value;

        protected (uint id, T value) ElementAndIdAtInternal(int index)
            => _list[index];

        protected int IndexOfInternal(T item)
            => _list.FindIndex(x => Equals(x.value, item));

        protected bool ContainsInternal(T item)
            => _list.Any(x => Equals(x.value, item));

        protected override IEnumerable<ListOp<T>> GetInitializationOperations()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                var elementData = _list[i];
                yield return new ListOp<T>() { source = this, element = elementData.value, elementId = elementData.id, index = i, isRemove = false };
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

        public IDisposable Subscribe(IListObserver<T> observer, bool immediate = false, uint? priority = null)
            => AddObserver(observer, immediate, priority);

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