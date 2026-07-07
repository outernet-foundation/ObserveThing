using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public enum OpType
    {
        Add,
        Remove
    }

    public interface ICollectionOperation : IOperation
    {
        object element { get; }
        uint elementId { get; }
        OpType opType { get; }
    }

    public interface ICollectionOperation<out T> : ICollectionOperation
    {
        new T element { get; }
        object ICollectionOperation.element => element;
    }

    public class ObservableCollectionBase<T> : Observable<ICollectionOperation<T>>, ICollectionObservable<T>
    {
        private class CollectionOperation : ICollectionOperation<T>
        {
            public IObservable source { get; set; }
            public uint elementId { get; set; }
            public OpType opType { get; set; }
            public T element { get; set; }

            public void Reset()
            {
                source = default;
                elementId = default;
                opType = default;
                element = default;
            }

            public IOperation Clone()
            {
                return new CollectionOperation()
                {
                    source = source,
                    elementId = elementId,
                    opType = opType,
                    element = element,
                };
            }
        }

        private Dictionary<uint, T> _collection = new Dictionary<uint, T>();

        public ObservableCollectionBase(ObservationContext context) : base(context) { }

        private CollectionOperation AllocateOperation(uint id, OpType opType, T element)
        {
            var op = context.AllocatePooledOperation<CollectionOperation>();
            op.source = this;
            op.elementId = id;
            op.opType = opType;
            op.element = element;
            return op;
        }

        protected override IReadOnlyList<ICollectionOperation<T>> GetInitializationOperations()
            => _collection.Select(x => AllocateOperation(x.Key, OpType.Add, x.Value)).ToArray();

        protected override void OnOperationNotificationsCompleted(ICollectionOperation<T> operation)
        {
            var op = (CollectionOperation)operation;
            op.Reset();
            context.DeallocatePooledOperation(op);
        }

        protected IEnumerable<(uint id, T element)> GetElementsWithIdsInternal()
            => _collection.Select<KeyValuePair<uint, T>, (uint id, T element)>(x => new(x.Key, x.Value));

        protected int GetCountInternal() => _collection.Count;

        protected uint AddInternal(uint id, T element)
        {
            _collection.Add(id, element);
            EnqueuePendingOperation(AllocateOperation(id, OpType.Add, element));
            return id;
        }

        protected bool RemoveInternal(uint id)
        {
            if (!_collection.TryGetValue(id, out var element))
                return false;

            _collection.Remove(id);
            EnqueuePendingOperation(AllocateOperation(id, OpType.Remove, element));
            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _collection.ToArray())
            {
                _collection.Remove(kvp.Key);
                EnqueuePendingOperation(AllocateOperation(kvp.Key, OpType.Remove, kvp.Value));
            }
        }

        public bool ContainsId(uint id)
            => _collection.ContainsKey(id);

        public bool Contains(T element)
            => _collection.ContainsValue(element);

        public IDisposable Subscribe(IObserver<ICollectionOperation> observer)
            => Subscribe(new Observer<ICollectionOperation<T>>(
                overridePriority: observer.overridePriority,
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}