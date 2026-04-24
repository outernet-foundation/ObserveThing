using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct ListOpArgs<T>
    {
        public uint id { get; }
        public int index { get; }
        public T element { get; }
        public bool isRemove { get; }

        public ListOpArgs(uint id, int index, T element, bool isRemove)
        {
            this.id = id;
            this.index = index;
            this.element = element;
            this.isRemove = isRemove;
        }
    }

    public class ObservableListBase<T> : Observable<ListOpArgs<T>>, IListObservable<T>
    {
        private List<(uint id, T value)> _list = new List<(uint id, T value)>();
        private CollectionIdProvider _idProvider;
        private List<ListOpArgs<T>> _initOps = new List<ListOpArgs<T>>();

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

        protected override IReadOnlyList<ListOpArgs<T>> GetInitializationOperations()
        {
            _initOps.Clear();
            for (int i = 0; i < _list.Count; i++)
            {
                var op = _list[i];
                _initOps.Add(new ListOpArgs<T>(op.id, i, op.value, false));
            }

            return _initOps;
        }

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
            EnqueuePendingOperation(new ListOpArgs<T>(removed.id, index, removed.value, true));
        }

        protected void InsertInternal(int index, T item)
        {
            (uint id, T value) inserted = new(_idProvider.GetUnusedId(), item);
            _list.Insert(index, inserted);
            EnqueuePendingOperation(new ListOpArgs<T>(inserted.id, index, inserted.value, false));
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

        public IDisposable Subscribe(IListObserver<T> observer)
            => Subscribe(
                new Observer<ListOpArgs<T>>(
                    onOperation: ops =>
                    {
                        foreach (var op in ops)
                        {
                            if (op.isRemove)
                            {
                                observer.OnRemove(op.id, op.index, op.element);
                            }
                            else
                            {
                                observer.OnAdd(op.id, op.index, op.element);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                )
            );

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(
                new Observer<ListOpArgs<T>>(
                    onOperation: ops =>
                    {
                        foreach (var op in ops)
                        {
                            if (op.isRemove)
                            {
                                observer.OnRemove(op.id, op.element);
                            }
                            else
                            {
                                observer.OnAdd(op.id, op.element);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                )
            );
    }
}