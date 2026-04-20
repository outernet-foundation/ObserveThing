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

    public class ListObservable<T> : IOperationObservable, IListObservable<T>, IEnumerable<T>, IDisposable
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
        private ObservationContext _context;
        private CollectionIdProvider _idProvider;

        public ListObservable(params T[] source) : this(source, default) { }
        public ListObservable(ObservationContext context, params T[] source) : this(source, context) { }
        public ListObservable(IEnumerable<T> source, ObservationContext context = default) : this(context)
        {
            foreach (var element in source)
                _list.Add(new(_idProvider.GetUnusedId(), element));
        }

        public ListObservable(ObservationContext context = default)
        {
            _context = context ?? ObservationContext.Default;
            _idProvider = new CollectionIdProvider(x => _list.Any(item => item.id == x));
        }

        IDisposable IListObservable<T>.Subscribe(IListObserver<T> observer)
            => _context.RegisterObserver(
                new OperationObserver(
                    onNext: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            for (int i = 0; i < _list.Count; i++)
                            {
                                var element = _list[i];
                                observer.OnAdd(element.id, i, element.value);
                            }

                            return;
                        }

                        foreach (var op in ops.Cast<IOperation<ListOpArgs<T>>>())
                        {
                            if (op.value.isRemove)
                            {
                                observer.OnRemove(op.value.id, op.value.index, op.value.element);
                            }
                            else
                            {
                                observer.OnAdd(op.value.id, op.value.index, op.value.element);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                ),
                this
            );

        IDisposable ICollectionObservable<T>.Subscribe(ICollectionObserver<T> observer)
            => _context.RegisterObserver(
                new OperationObserver(
                    onNext: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            foreach (var element in _list)
                                observer.OnAdd(element.id, element.value);

                            return;
                        }

                        foreach (var op in ops.Cast<IOperation<ListOpArgs<T>>>())
                        {
                            if (op.value.isRemove)
                            {
                                observer.OnRemove(op.value.id, op.value.element);
                            }
                            else
                            {
                                observer.OnAdd(op.value.id, op.value.element);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                ),
                this
            );

        public IDisposable Subscribe(IOperationObserver observer)
            => _context.RegisterObserver(observer, this);

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
            _context.RegisterOperation(this, new ListOpArgs<T>(removed.id, index, removed.value, true));
        }

        public void Insert(int index, T item)
        {
            (uint id, T value) inserted = new(_idProvider.GetUnusedId(), item);
            _list.Insert(index, inserted);
            _context.RegisterOperation(this, new ListOpArgs<T>(inserted.id, index, inserted.value, false));
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

        public void Dispose()
        {
            _context.HandleObservableDisposed(this);
        }
    }
}