using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface ICollectionOp : IOperation
    {
        object element { get; }
        uint elementId { get; }
        bool isRemove { get; }
    }

    public struct CollectionOp<T> : ICollectionOp
    {
        public IObservable<IOperation> source { get; set; }
        public T element { get; set; }
        public uint elementId { get; set; }
        public bool isRemove { get; set; }

        object ICollectionOp.element => element;
    }

    public class ObservableCollectionBase<T> : ObservableBase<ICollectionObserver<T>, CollectionOp<T>>, ICollectionObservable<T>
    {
        private Dictionary<uint, T> _collection = new Dictionary<uint, T>();

        public ObservableCollectionBase(ObservationContext context) : base(context) { }

        protected IEnumerable<(uint id, T element)> GetElementsWithIdsInternal()
            => _collection.Select<KeyValuePair<uint, T>, (uint id, T element)>(x => new(x.Key, x.Value));

        protected int GetCountInternal() => _collection.Count;

        protected uint AddInternal(uint id, T element)
        {
            _collection.Add(id, element);
            EnqueuePendingOperation(new CollectionOp<T>() { source = this, elementId = id, element = element, isRemove = false });
            return id;
        }

        protected bool RemoveInternal(uint id)
        {
            if (!_collection.TryGetValue(id, out var element))
                return false;

            _collection.Remove(id);
            EnqueuePendingOperation(new CollectionOp<T>() { source = this, elementId = id, element = element, isRemove = true });
            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _collection.ToArray())
            {
                _collection.Remove(kvp.Key);
                EnqueuePendingOperation(new CollectionOp<T>() { source = this, elementId = kvp.Key, element = kvp.Value, isRemove = true });
            }
        }

        protected override IEnumerable<CollectionOp<T>> GetInitializationOperations()
        {
            foreach (var elementData in _collection)
                yield return new CollectionOp<T>() { source = this, element = elementData.Value, elementId = elementData.Key, isRemove = false };
        }

        protected override void SendOperation(ICollectionObserver<T> observer, CollectionOp<T> operation)
        {
            if (operation.isRemove)
            {
                observer.OnRemove(operation.elementId, operation.element);
            }
            else
            {
                observer.OnAdd(operation.elementId, operation.element);
            }
        }

        public bool ContainsId(uint id)
            => _collection.ContainsKey(id);

        public bool Contains(T element)
            => _collection.ContainsValue(element);

        public IDisposable Subscribe(ICollectionObserver<T> observer, bool immediate = false, uint? priority = null)
            => AddObserver(observer, immediate, priority);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new CollectionObserver<T>(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}