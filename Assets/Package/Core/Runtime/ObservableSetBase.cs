using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct SetOpArgs<T>
    {
        public uint id { get; }
        public T element { get; }
        public bool isRemove { get; }

        public SetOpArgs(uint id, T element, bool isRemove)
        {
            this.id = id;
            this.element = element;
            this.isRemove = isRemove;
        }
    }

    public class ObservableSetBase<T> : Observable<SetOpArgs<T>>, ISetObservable<T>
    {
        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private CollectionIdProvider _idProvider;
        private List<SetOpArgs<T>> _initOps = new List<SetOpArgs<T>>();

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

        protected override IReadOnlyList<SetOpArgs<T>> GetInitializationOperations()
        {
            _initOps.Clear();
            _initOps.AddRange(_set.Select(x => new SetOpArgs<T>(x.Value, x.Key, false)));
            return _initOps;
        }

        protected bool AddInternal(T element)
        {
            if (_set.ContainsKey(element))
                return false;

            var id = _idProvider.GetUnusedId();
            _set.Add(element, id);
            EnqueuePendingOperation(new SetOpArgs<T>(id, element, false));
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
            EnqueuePendingOperation(new SetOpArgs<T>(id, element, true));

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                EnqueuePendingOperation(new SetOpArgs<T>(kvp.Value, kvp.Key, true));
            }
        }

        protected bool ContainsInternal(T element)
            => _set.ContainsKey(element);

        public IDisposable Subscribe(ISetObserver<T> observer)
            => Subscribe(
                new Observer<SetOpArgs<T>>(
                    onOperation: ops =>
                    {
                        foreach (var op in ops)
                        {
                            if (op.isRemove)
                            {
                                observer.OnRemove(op.id, op.element);
                            }
                            else
                            {
                                observer.OnAdd(op.id, op.element);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                )
            );

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(
                new Observer<SetOpArgs<T>>(
                    onOperation: ops =>
                    {
                        foreach (var op in ops)
                        {
                            if (op.isRemove)
                            {
                                observer.OnRemove(op.id, op.element);
                            }
                            else
                            {
                                observer.OnAdd(op.id, op.element);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                )
            );
    }
}