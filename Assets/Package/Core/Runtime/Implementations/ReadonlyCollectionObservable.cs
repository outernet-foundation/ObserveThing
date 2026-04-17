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

    public class ReadonlyCollectionObservable<T> : IObservable, ICollectionOperator<T>, IEnumerable<T>, IDisposable
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _initOperations.Select(x => x.value.element).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _initOperations.Select(x => x.value.element).GetEnumerator();

        public int count => _initOperations.Count;

        private ObservationContext _context;
        private List<ObservableOperation<CollectionOp<T>>> _initOperations = new List<ObservableOperation<CollectionOp<T>>>();

        public ReadonlyCollectionObservable(params T[] source) : this((IEnumerable<T>)source) { }
        public ReadonlyCollectionObservable(ObservationContext context, params T[] source) : this(source, context) { }

        public ReadonlyCollectionObservable(IEnumerable<T> source, ObservationContext context = default)
        {
            _context = context ?? ObservationContext.Default;

            uint nextId = 0;

            foreach (var element in source)
            {
                _initOperations.Add(new ObservableOperation<CollectionOp<T>>() { source = this, value = new CollectionOp<T>(nextId, -1, element, false) });
                nextId++;
            }
        }

        void IObservable.InitializeObserver(IObserver observer)
        {
            observer.OnNext(_initOperations);
        }

        IDisposable ICollectionOperator<T>.Subscribe(ICollectionObserver<T> observer)
            => _context.RegisterObserver(
                new Observer(
                    onNext: ops =>
                    {
                        foreach (var op in ops.Cast<IObservableOperation<CollectionOp<T>>>())
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

        public bool Contains(T element)
            => _initOperations.Select(x => x.value.element).Contains(element);

        public void Dispose()
        {
            _context.HandleObservableDisposed(this);
        }
    }
}