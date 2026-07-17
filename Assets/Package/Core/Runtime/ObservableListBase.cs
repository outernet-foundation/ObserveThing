using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableListBase<T> : Observable<CollectionOp<ListData<T>>>
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

        public override IReadOnlyList<CollectionOp<ListData<T>>> GetInitializationOperations()
            => _list.Select((element, index) => new CollectionOp<ListData<T>>()
            {
                opType = OpType.Add,
                elementId = element.id,
                value = new ListData<T>() { element = element.value, index = index }
            }).ToArray();

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
            EnqueuePendingOperation(new CollectionOp<ListData<T>>()
            {
                opType = OpType.Remove,
                elementId = removed.id,
                value = new ListData<T>() { element = removed.value, index = index }
            });
        }

        protected void InsertInternal(int index, T item)
        {
            (uint id, T value) inserted = new(_idProvider.GetUnusedId(), item);
            _list.Insert(index, inserted);
            EnqueuePendingOperation(new CollectionOp<ListData<T>>()
            {
                opType = OpType.Add,
                elementId = inserted.id,
                value = new ListData<T>() { element = inserted.value, index = index }
            });
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
    }
}