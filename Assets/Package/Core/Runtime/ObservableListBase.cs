using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface IListOperation : ICollectionOperation
    {
        int index { get; }
    }

    public interface IListOperation<out T> : IListOperation, ICollectionOperation<T> { }

    public class ObservableListBase<T> : Observable<IListOperation<T>>, IListObservable<T>
    {
        private class ListOperation : IListOperation<T>
        {
            public IObservable source { get; set; }
            public uint elementId { get; set; }
            public OpType opType { get; set; }
            public int index { get; set; }
            public T element { get; set; }

            public void Reset()
            {
                source = default;
                elementId = default;
                opType = default;
                index = default;
                element = default;
            }

            public IOperation Clone()
            {
                return new ListOperation()
                {
                    source = source,
                    elementId = elementId,
                    opType = opType,
                    index = index,
                    element = element,
                };
            }
        }

        private List<(uint id, T value)> _list = new List<(uint id, T value)>();
        private CollectionIdProvider _idProvider;

        public ObservableListBase(ObservationContext context) : this(context, null) { }
        public ObservableListBase(ObservationContext context, IEnumerable<T> value) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _list.Any(item => item.id == x));

            if (value == null)
                return;

            foreach (var element in value)
                _list.Add(new(_idProvider.GetUnusedId(), element));
        }

        private ListOperation AllocateOperation(uint id, int index, OpType opType, T element)
        {
            var op = context.AllocatePooledOperation<ListOperation>();
            op.source = this;
            op.elementId = id;
            op.index = index;
            op.opType = opType;
            op.element = element;
            return op;
        }

        protected int GetCountInternal()
            => _list.Count;

        protected IEnumerable<(uint id, T value)> ElementsInternal()
            => _list;

        protected override IReadOnlyList<IListOperation<T>> GetInitializationOperations()
            => _list.Select((element, index) => AllocateOperation(element.id, index, OpType.Add, element.value)).ToArray();

        protected override void OnOperationNotificationsCompleted(IListOperation<T> operation)
        {
            var op = (ListOperation)operation;
            op.Reset();
            context.DeallocatePooledOperation(op);
        }

        protected void AddInternal(T added)
            => InsertInternal(_list.Count, added);

        protected void AddRangeInternal(IEnumerable<T> toAdd)
        {
            foreach (var added in toAdd)
                AddInternal(added);
        }

        protected bool RemoveInternal(T removed)
        {
            var index = _list.FindIndex(x => Equals(x.value, removed));

            if (index == -1)
                return false;

            RemoveAtInternal(index);
            return true;
        }

        protected void RemoveAtInternal(int index)
        {
            var removed = _list[index];
            _list.RemoveAt(index);
            EnqueuePendingOperation(AllocateOperation(removed.id, index, OpType.Remove, removed.value));
        }

        protected void InsertInternal(int index, T item)
        {
            (uint id, T value) inserted = new(_idProvider.GetUnusedId(), item);
            _list.Insert(index, inserted);
            EnqueuePendingOperation(AllocateOperation(inserted.id, index, OpType.Add, inserted.value));
        }

        protected void ClearInternal()
        {
            while (_list.Count > 0)
                RemoveAtInternal(_list.Count - 1);
        }

        protected T ElementAtInternal(int index)
            => _list[index].value;

        protected (uint id, T value) ElementAndIdAtInternal(int index)
            => _list[index];

        protected int IndexOfInternal(T item)
            => _list.FindIndex(x => Equals(x.value, item));

        protected bool ContainsInternal(T item)
            => _list.Any(x => Equals(x.value, item));

        public IDisposable Subscribe(IObserver<IListOperation> observer)
            => Subscribe(new Observer<IListOperation<T>>(
                overridePriority: observer.overridePriority,
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver<ICollectionOperation<T>> observer)
            => Subscribe(new Observer<IListOperation<T>>(
                overridePriority: observer.overridePriority,
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver<ICollectionOperation> observer)
            => Subscribe(new Observer<IListOperation<T>>(
                overridePriority: observer.overridePriority,
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}