using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableCollectionBase<T> : Observable<CollectionOp<T>>
    {
        private Dictionary<uint, T> _collection = new Dictionary<uint, T>();

        public ObservableCollectionBase(ObservationContext context) : base(context) { }

        public override IReadOnlyList<CollectionOp<T>> GetInitializationOperations()
            => _collection.Select(x => new CollectionOp<T>() { opType = OpType.Add, elementId = x.Key, value = x.Value }).ToArray();

        protected IEnumerable<(uint id, T element)> GetElementsWithIdsInternal()
            => _collection.Select<KeyValuePair<uint, T>, (uint id, T element)>(x => new(x.Key, x.Value));

        protected int GetCountInternal() => _collection.Count;

        protected uint AddInternal(uint id, T element)
        {
            _collection.Add(id, element);
            EnqueuePendingOperation(new CollectionOp<T>() { opType = OpType.Add, elementId = id, value = element });
            return id;
        }

        protected bool RemoveInternal(uint id)
        {
            if (!_collection.TryGetValue(id, out var element))
                return false;

            _collection.Remove(id);
            EnqueuePendingOperation(new CollectionOp<T>() { opType = OpType.Remove, elementId = id, value = element });
            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _collection.ToArray())
            {
                _collection.Remove(kvp.Key);
                EnqueuePendingOperation(new CollectionOp<T>() { opType = OpType.Remove, elementId = kvp.Key, value = kvp.Value });
            }
        }

        public bool ContainsId(uint id)
            => _collection.ContainsKey(id);

        public bool Contains(T element)
            => _collection.ContainsValue(element);
    }
}