using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct CollectionOpArgs<T>
    {
        public uint id { get; }
        public T element { get; }
        public bool isRemove { get; }

        public CollectionOpArgs(uint id, T element, bool isRemove)
        {
            this.id = id;
            this.element = element;
            this.isRemove = isRemove;
        }
    }

    public class ObservableCollectionBase<T> : Observable<CollectionOpArgs<T>>, ICollectionObservable<T>
    {
        private Dictionary<uint, T> _collection = new Dictionary<uint, T>();
        private List<CollectionOpArgs<T>> _initOps = new List<CollectionOpArgs<T>>();

        public ObservableCollectionBase(ObservationContext context) : base(context) { }

        protected override IReadOnlyList<CollectionOpArgs<T>> GetInitializationOperations()
        {
            _initOps.Clear();
            _initOps.AddRange(_collection.Select(x => new CollectionOpArgs<T>(x.Key, x.Value, false)));
            return _initOps;
        }

        protected IEnumerable<(uint id, T element)> GetElementsWithIdsInternal()
            => _collection.Select<KeyValuePair<uint, T>, (uint id, T element)>(x => new(x.Key, x.Value));

        protected int GetCountInternal() => _collection.Count;

        protected uint AddInternal(uint id, T element)
        {
            _collection.Add(id, element);
            EnqueuePendingOperation(new CollectionOpArgs<T>(id, element, false));
            return id;
        }

        protected bool RemoveInternal(uint id)
        {
            if (!_collection.TryGetValue(id, out var element))
                return false;

            _collection.Remove(id);
            EnqueuePendingOperation(new CollectionOpArgs<T>(id, element, true));
            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _collection.ToArray())
            {
                _collection.Remove(kvp.Key);
                EnqueuePendingOperation(new CollectionOpArgs<T>(kvp.Key, kvp.Value, true));
            }
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(
                new Observer<CollectionOpArgs<T>>(
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

        public bool ContainsId(uint id)
            => _collection.ContainsKey(id);

        public bool Contains(T element)
            => _collection.ContainsValue(element);
    }
}