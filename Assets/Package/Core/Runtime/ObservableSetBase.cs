using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface ISetOperation : ICollectionOperation { }

    public interface ISetOperation<out T> : ISetOperation, ICollectionOperation<T> { }

    public class ObservableSetBase<T> : Observable<ISetOperation<T>>, ISetObservable<T>
    {
        private class SetOperation : ISetOperation<T>
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
        }

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

        private SetOperation AllocateOperation(uint id, OpType opType, T element)
        {
            var op = context.AllocatePooledOperation<SetOperation>();
            op.source = this;
            op.elementId = id;
            op.opType = opType;
            op.element = element;
            return op;
        }

        protected override IReadOnlyList<ISetOperation<T>> GetInitializationOperations()
            => _set.Select(x => AllocateOperation(x.Value, OpType.Add, x.Key)).ToArray();

        protected override void HandleOperationNotificationsComplete(ISetOperation<T> operation)
        {
            var op = (SetOperation)operation;
            op.Reset();
            context.DeallocatePooledOperation(op);
        }

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
            EnqueuePendingOperation(AllocateOperation(id, OpType.Add, element));
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
            EnqueuePendingOperation(AllocateOperation(id, OpType.Add, element));

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                EnqueuePendingOperation(AllocateOperation(kvp.Value, OpType.Remove, kvp.Key));
            }
        }

        protected bool ContainsInternal(T element)
            => _set.ContainsKey(element);

        public IDisposable Subscribe(IObserver<ICollectionOperation<T>> observer)
            => Subscribe(new Observer<ISetOperation<T>>(
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver<ICollectionOperation> observer)
            => Subscribe(new Observer<ISetOperation<T>>(
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}