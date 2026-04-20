using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct SetOpArgs<T>
    {
        public uint id { get; }
        public T element { get; }
        public bool isRemove { get; }

        public SetOpArgs(uint id, T element, bool isRemove)
        {
            this.id = id;
            this.element = element;
            this.isRemove = isRemove;
        }
    }

    public class SetObservable<T> : IOperationObservable, ISetObservable<T>, IEnumerable<T>, IDisposable
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => _set.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _set.Keys.GetEnumerator();

        public int count => _set.Count;

        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private ObservationContext _context;
        private CollectionIdProvider _idProvider;

        public SetObservable(params T[] source) : this(source, default) { }
        public SetObservable(ObservationContext context, params T[] source) : this(source, context) { }
        public SetObservable(IEnumerable<T> values, ObservationContext context = default) : this(context)
        {
            foreach (T value in values)
                _set.Add(value, _idProvider.GetUnusedId());
        }

        public SetObservable(ObservationContext context = default)
        {
            _context = context ?? ObservationContext.Default;
            _idProvider = new CollectionIdProvider(x => _set.ContainsValue(x));
        }

        IDisposable ISetObservable<T>.Subscribe(ISetObserver<T> observer)
            => _context.RegisterObserver(
                new OperationObserver(
                    onNext: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            foreach (var element in _set)
                                observer.OnAdd(element.Value, element.Key);
                        }

                        foreach (var op in ops.Cast<IOperation<SetOpArgs<T>>>())
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

        IDisposable ICollectionObservable<T>.Subscribe(ICollectionObserver<T> observer)
            => _context.RegisterObserver(
                new OperationObserver(
                    onNext: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            foreach (var element in _set)
                                observer.OnAdd(element.Value, element.Key);
                        }

                        foreach (var op in ops.Cast<IOperation<SetOpArgs<T>>>())
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

        public bool Add(T element)
        {
            if (_set.ContainsKey(element))
                return false;

            var id = _idProvider.GetUnusedId();
            _set.Add(element, id);
            _context.RegisterOperation(this, new SetOpArgs<T>(id, element, false));
            return true;
        }

        public void AddRange(IEnumerable<T> elements)
        {
            foreach (var element in elements)
                Add(element);
        }

        public bool Remove(T element)
        {
            if (!_set.TryGetValue(element, out var id))
                return false;

            _set.Remove(element);
            _context.RegisterOperation(this, new SetOpArgs<T>(id, element, true));

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                _context.RegisterOperation(this, new SetOpArgs<T>(kvp.Value, kvp.Key, true));
            }
        }

        public bool Contains(T element)
            => _set.ContainsKey(element);

        public void Dispose()
        {
            _context.HandleObservableDisposed(this);
        }
    }
}