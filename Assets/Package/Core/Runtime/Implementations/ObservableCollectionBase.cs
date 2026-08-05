using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct CollectionOpArgs<T>
    {
        public T element;
        public uint elementId;
        public bool isRemove;
    }

    public class ObservableCollectionBase<T> : ObservableBase<ICollectionObserver<T>, CollectionOpArgs<T>>, ICollectionObservable<T>
    {
        private Dictionary<uint, T> _collection = new Dictionary<uint, T>();
        private Stack<Operation> _operationPool = new Stack<Operation>();

        public ObservableCollectionBase(ObservationContext context) : base(context) { }

        protected IEnumerable<(uint id, T element)> GetElementsWithIdsInternal()
            => _collection.Select<KeyValuePair<uint, T>, (uint id, T element)>(x => new(x.Key, x.Value));

        protected int GetCountInternal() => _collection.Count;

        protected uint AddInternal(uint id, T element)
        {
            _collection.Add(id, element);
            EnqueuePendingOperation(new CollectionOpArgs<T>() { elementId = id, element = element, isRemove = false });
            return id;
        }

        protected bool RemoveInternal(uint id)
        {
            if (!_collection.TryGetValue(id, out var element))
                return false;

            _collection.Remove(id);
            EnqueuePendingOperation(new CollectionOpArgs<T>() { elementId = id, element = element, isRemove = true });
            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _collection.ToArray())
            {
                _collection.Remove(kvp.Key);
                EnqueuePendingOperation(new CollectionOpArgs<T>() { elementId = kvp.Key, element = kvp.Value, isRemove = true });
            }
        }

        protected override void SendOperation(ICollectionObserver<T> observer, CollectionOpArgs<T> operation)
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
        {
            var subscription = AddObserver(observer, immediate, priority);
            foreach (var element in _collection)
                observer.OnAdd(element.Key, element.Value);
            return subscription;
        }

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new CollectionObserver<T>(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        public IDisposable Subscribe(IObserver observer, bool immediate = false, uint? priority = null)
            => Subscribe(new CollectionObserver<T>(
                onAdd: (id, element) =>
                {
                    var operation = _operationPool.TryPop(out var op) ? op : new Operation(this);
                    operation.args = new SetOpArgs<T>() { elementId = id, element = element, isRemove = false };
                    observer.OnNext(operation);
                    _operationPool.Push(operation);
                },
                onRemove: (id, element) =>
                {
                    var operation = _operationPool.TryPop(out var op) ? op : new Operation(this);
                    operation.args = new SetOpArgs<T>() { elementId = id, element = element, isRemove = true };
                    observer.OnNext(operation);
                    _operationPool.Push(operation);
                },
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}