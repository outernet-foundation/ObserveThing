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

    public class SetObservable<T> : Observable<SetOpArgs<T>>, ISetObservable<T>, IEnumerable<T>, IDisposable
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => _set.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _set.Keys.GetEnumerator();

        public int count => _set.Count;

        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private CollectionIdProvider _idProvider;

        public SetObservable(params T[] source) : this(source, default) { }
        public SetObservable(ObservationContext context, params T[] source) : this(source, context) { }
        public SetObservable(IEnumerable<T> values, ObservationContext context = default) : this(context)
        {
            foreach (T value in values)
                _set.Add(value, _idProvider.GetUnusedId());
        }

        public SetObservable(ObservationContext context = default) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _set.ContainsValue(x));
        }

        public IDisposable Subscribe(ISetObserver<T> observer)
            => Subscribe(
                new Observer<SetOpArgs<T>>(
                    onOperation: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            foreach (var element in _set)
                                observer.OnAdd(element.Value, element.Key);
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

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(
                new Observer<SetOpArgs<T>>(
                    onOperation: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            foreach (var element in _set)
                                observer.OnAdd(element.Value, element.Key);
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

        public bool Add(T element)
        {
            if (_set.ContainsKey(element))
                return false;

            var id = _idProvider.GetUnusedId();
            _set.Add(element, id);
            EnqueuePendingOperation(new SetOpArgs<T>(id, element, false));
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
            EnqueuePendingOperation(new SetOpArgs<T>(id, element, true));

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                EnqueuePendingOperation(new SetOpArgs<T>(kvp.Value, kvp.Key, true));
            }
        }

        public bool Contains(T element)
            => _set.ContainsKey(element);
    }
}