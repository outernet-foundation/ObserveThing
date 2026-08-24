using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public interface ISetOp : IOperation
    {
        uint elementId { get; }
        object element { get; }
        bool isRemove { get; }
    }

    public struct SetOp<T> : ISetOp
    {
        public IObservable<IOperation> source { get; set; }
        public uint elementId { get; set; }
        public T element { get; set; }
        public bool isRemove { get; set; }

        object ISetOp.element => element;
    }

    public class ObservableSet<T> : ObservableBase<ISetObserver<T>, SetOp<T>>, ISetObservable<T>, IEnumerable<T>
    {
        public int count => _set.Count;

        public IEnumerator<T> GetEnumerator()
            => _set.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _set.Keys.GetEnumerator();

        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private CollectionIdProvider _idProvider;

        public ObservableSet() : this(default, default) { }
        public ObservableSet(IEnumerable<T> values) : this(default, values) { }
        public ObservableSet(ObservationContext context) : this(context, null) { }
        public ObservableSet(ObservationContext context, IEnumerable<T> values) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _set.ContainsValue(x));

            if (values == null)
                return;

            foreach (T value in values)
                _set.Add(value, _idProvider.GetUnusedId());
        }

        public bool Add(T element)
        {
            if (_set.ContainsKey(element))
                return false;

            var id = _idProvider.GetUnusedId();
            _set.Add(element, id);
            EnqueuePendingOperation(new SetOp<T>() { source = this, elementId = id, element = element, isRemove = false });
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
            EnqueuePendingOperation(new SetOp<T>() { source = this, elementId = id, element = element, isRemove = true });

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                EnqueuePendingOperation(new SetOp<T>() { source = this, elementId = kvp.Value, element = kvp.Key, isRemove = true });
            }
        }

        public bool Contains(T element)
            => _set.ContainsKey(element);

        public uint GetElementId(T element)
            => _set[element];

        protected override IEnumerable<SetOp<T>> GetInitializationOperations()
        {
            foreach (var element in _set)
                yield return new SetOp<T>() { source = this, element = element.Key, elementId = element.Value, isRemove = false };
        }

        protected override void SendOperation(ISetObserver<T> observer, SetOp<T> operation)
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

        IDisposable ISetObservable.Subscribe(ISetObserver observer, bool immediate, uint? priority)
            => Subscribe(new SetObserver<T>(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable<T>.Subscribe(ICollectionObserver<T> observer, bool immediate, uint? priority)
            => Subscribe(new SetObserver<T>(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new SetObserver<T>(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}