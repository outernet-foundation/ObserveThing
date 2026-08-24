using System;
using System.Collections;
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

    public class ObservableCollection<T> : ObservableBase<ICollectionObserver<T>, CollectionOp<T>>, ICollectionObservable<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => _collection.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _collection.Values.GetEnumerator();

        public int Count => _collection.Count;

        private Dictionary<uint, T> _collection = new Dictionary<uint, T>();

        private CollectionIdProvider _idProvider;

        public ObservableCollection() : this(default, default(IEnumerable<T>)) { }
        public ObservableCollection(params T[] value) : this(default, (IEnumerable<T>)value) { }
        public ObservableCollection(IEnumerable<T> value) : this(default, value) { }

        public ObservableCollection(ObservationContext context) : this(context, default(IEnumerable<T>)) { }
        public ObservableCollection(ObservationContext context, params T[] value) : this(context, (IEnumerable<T>)value) { }
        public ObservableCollection(ObservationContext context, IEnumerable<T> value) : base(context)
        {
            _idProvider = new CollectionIdProvider(_collection.ContainsKey);

            if (value == null)
                return;

            foreach (var element in value)
            {
                var id = _idProvider.GetUnusedId();
                _collection.Add(id, element);
            }
        }

        public bool Contains(T element)
            => _collection.ContainsValue(element);

        public uint Add(T element)
        {
            var id = _idProvider.GetUnusedId();
            Add(id, element);
            return id;
        }

        public void Add(uint id, T element)
        {
            _collection.Add(id, element);
            EnqueuePendingOperation(new CollectionOp<T>() { source = this, elementId = id, element = element, isRemove = false });
        }

        public bool Remove(T element)
        {
            var id = default(uint);
            var found = false;

            foreach (var kvp in _collection)
            {
                if (!Equals(element, kvp.Value))
                    continue;

                id = kvp.Key;
                found = true;
                break;
            }

            if (!found)
                return false;

            return Remove(id);
        }

        public bool Remove(uint id)
        {
            if (!_collection.TryGetValue(id, out var element))
                return false;

            _collection.Remove(id);
            EnqueuePendingOperation(new CollectionOp<T>() { source = this, elementId = id, element = element, isRemove = true });
            return true;
        }

        public void Clear()
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

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new CollectionObserver<T>(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}