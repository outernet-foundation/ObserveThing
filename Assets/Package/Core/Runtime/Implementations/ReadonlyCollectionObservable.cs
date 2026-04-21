using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct CollectionOpArgs<T>
    {
        public uint id { get; }
        public int index { get; }
        public T element { get; }
        public bool isRemove { get; }

        public CollectionOpArgs(uint id, int index, T element, bool isRemove)
        {
            this.id = id;
            this.index = index;
            this.element = element;
            this.isRemove = isRemove;
        }
    }

    public class ReadonlyCollectionObservable<T> : Observable<CollectionOpArgs<T>>, ICollectionObservable<T>, IEnumerable<T>, IDisposable
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _collection.Select(x => x.element).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _collection.Select(x => x.element).GetEnumerator();

        public int count => _collection.Count;

        private List<(uint id, T element)> _collection = new List<(uint id, T element)>();

        public ReadonlyCollectionObservable(params T[] source) : this((IEnumerable<T>)source) { }
        public ReadonlyCollectionObservable(ObservationContext context, params T[] source) : this(source, context) { }

        public ReadonlyCollectionObservable(IEnumerable<T> source, ObservationContext context = default) : base(context)
        {
            uint nextId = 0;

            foreach (var element in source)
            {
                _collection.Add(new(nextId, element));
                nextId++;
            }
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(
                new Observer<CollectionOpArgs<T>>(
                    onOperation: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            foreach (var element in _collection)
                                observer.OnAdd(element.id, element.element);
                        }

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

        public bool Contains(T element)
            => _collection.Select(x => x.element).Contains(element);
    }
}