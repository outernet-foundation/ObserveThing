using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableSetBase<T> : Observable<CollectionOp<T>>
    {
        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private CollectionIdProvider _idProvider;

        public ObservableSetBase(ObservationContext context) : this(context, null) { }
        public ObservableSetBase(ObservationContext context, IEnumerable<T> values) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _set.ContainsValue(x));

            if (values == null)
                return;

            foreach (T value in values)
                _set.Add(value, _idProvider.GetUnusedId());
        }

        public override IReadOnlyList<CollectionOp<T>> GetInitializationOperations()
            => _set.Select(x => new CollectionOp<T>() { opType = OpType.Add, elementId = x.Value, value = x.Key }).ToArray();

        protected int GetCountInternal()
            => _set.Count;

        protected IEnumerable<KeyValuePair<T, uint>> GetElementsInternal()
            => _set;

        protected bool AddInternal(T element)
        {
            if (_set.ContainsKey(element))
                return false;

            var id = _idProvider.GetUnusedId();
            _set.Add(element, id);
            EnqueuePendingOperation(new CollectionOp<T>() { opType = OpType.Add, elementId = id, value = element });
            return true;
        }

        protected void AddRangeInternal(IEnumerable<T> elements)
        {
            foreach (var element in elements)
                AddInternal(element);
        }

        protected bool RemoveInternal(T element)
        {
            if (!_set.TryGetValue(element, out var id))
                return false;

            _set.Remove(element);
            EnqueuePendingOperation(new CollectionOp<T>() { opType = OpType.Remove, elementId = id, value = element });

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                EnqueuePendingOperation(new CollectionOp<T>() { opType = OpType.Remove, elementId = kvp.Value, value = kvp.Key });
            }
        }

        protected bool ContainsInternal(T element)
            => _set.ContainsKey(element);
    }
}