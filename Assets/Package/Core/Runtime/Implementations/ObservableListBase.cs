using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct ListOpArgs<T>
    {
        public int index;
        public uint elementId;
        public T element;
        public bool isRemove;
    }

    public class ObservableListBase<T> : ObservableBase<IListObserver<T>, ListOpArgs<T>>, IListObservable<T>
    {
        private List<(uint id, T value)> _list = new List<(uint id, T value)>();
        private CollectionIdProvider _idProvider;
        private Stack<Operation> _operationPool = new Stack<Operation>();

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
            EnqueuePendingOperation(new ListOpArgs<T>() { element = removed.value, elementId = removed.id, index = index, isRemove = true });
        }

        protected void InsertInternal(int index, T item)
        {
            (uint id, T value) inserted = new(_idProvider.GetUnusedId(), item);
            _list.Insert(index, inserted);
            EnqueuePendingOperation(new ListOpArgs<T>() { element = inserted.value, elementId = inserted.id, index = index, isRemove = false });
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

        protected override void SendOperation(IListObserver<T> observer, ListOpArgs<T> operation)
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
        {
            var subscription = AddObserver(observer, immediate, priority);

            for (int i = 0; i < _list.Count; i++)
            {
                var element = _list[i];
                observer.OnAdd(element.id, i, element.value);
            }

            return subscription;
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

        public IDisposable Subscribe(IObserver observer, bool immediate = false, uint? priority = null)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, index, element) =>
                {
                    var operation = _operationPool.TryPop(out var op) ? op : new Operation(this);
                    operation.args = new ListOpArgs<T>() { elementId = id, index = index, element = element, isRemove = false };
                    observer.OnNext(operation);
                    _operationPool.Push(operation);
                },
                onRemove: (id, index, element) =>
                {
                    var operation = _operationPool.TryPop(out var op) ? op : new Operation(this);
                    operation.args = new ListOpArgs<T>() { elementId = id, index = index, element = element, isRemove = true };
                    observer.OnNext(operation);
                    _operationPool.Push(operation);
                },
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}