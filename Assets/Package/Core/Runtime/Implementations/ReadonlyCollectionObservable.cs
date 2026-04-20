using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct CollectionOp<T>
    {
        public uint id { get; }
        public int index { get; }
        public T element { get; }
        public bool isRemove { get; }

        public CollectionOp(uint id, int index, T element, bool isRemove)
        {
            this.id = id;
            this.index = index;
            this.element = element;
            this.isRemove = isRemove;
        }
    }

    public class ReadonlyCollectionObservable<T> : IOperationObservable, ICollectionObservable<T>, IEnumerable<T>, IDisposable
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _collection.Select(x => x.element).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _collection.Select(x => x.element).GetEnumerator();

        public int count => _collection.Count;

        private ObservationContext _context;
        private List<(uint id, T element)> _collection = new List<(uint id, T element)>();

        public ReadonlyCollectionObservable(params T[] source) : this((IEnumerable<T>)source) { }
        public ReadonlyCollectionObservable(ObservationContext context, params T[] source) : this(source, context) { }

        public ReadonlyCollectionObservable(IEnumerable<T> source, ObservationContext context = default)
        {
            _context = context ?? ObservationContext.Default;

            uint nextId = 0;

            foreach (var element in source)
            {
                _collection.Add(new(nextId, element));
                nextId++;
            }
        }

        IDisposable ICollectionObservable<T>.Subscribe(ICollectionObserver<T> observer)
            => _context.RegisterObserver(
                new OperationObserver(
                    onNext: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            foreach (var element in _collection)
                                observer.OnAdd(element.id, element.element);
                        }

                        foreach (var op in ops.Cast<IOperation<CollectionOp<T>>>())
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

        public bool Contains(T element)
            => _collection.Select(x => x.element).Contains(element);

        public void Dispose()
        {
            _context.HandleObservableDisposed(this);
        }
    }
}