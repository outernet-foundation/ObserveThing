using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct SetOpArgs<T>
    {
        public uint elementId;
        public T element;
        public bool isRemove;
    }

    public class ObservableSetBase<T> : ObservableBase<ISetObserver<T>, SetOpArgs<T>>, ISetObservable<T>
    {
        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private CollectionIdProvider _idProvider;
        private Stack<Operation> _operationPool = new Stack<Operation>();

        public ObservableSetBase(ObservationContext context) : this(context, null) { }
        public ObservableSetBase(ObservationContext context, IEnumerable<T> values) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _set.ContainsValue(x));

            if (values == null)
                return;

            foreach (T value in values)
                _set.Add(value, _idProvider.GetUnusedId());
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
            EnqueuePendingOperation(new SetOpArgs<T>() { elementId = id, element = element, isRemove = false });
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
            EnqueuePendingOperation(new SetOpArgs<T>() { elementId = id, element = element, isRemove = true });

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                EnqueuePendingOperation(new SetOpArgs<T>() { elementId = kvp.Value, element = kvp.Key, isRemove = false });
            }
        }

        protected bool ContainsInternal(T element)
            => _set.ContainsKey(element);

        protected override void SendOperation(ISetObserver<T> observer, SetOpArgs<T> operation)
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

        public IDisposable Subscribe(ISetObserver<T> observer, bool immediate = false, uint? priority = null)
        {
            var subscription = AddObserver(observer, immediate, priority);
            foreach (var element in _set)
                observer.OnAdd(element.Value, element.Key);
            return subscription;
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

        public IDisposable Subscribe(IObserver observer, bool immediate = false, uint? priority = null)
            => Subscribe(new SetObserver<T>(
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