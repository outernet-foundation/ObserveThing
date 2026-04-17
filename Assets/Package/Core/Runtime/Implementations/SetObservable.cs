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

    public class SetObservable<T> : IObservable, ISetOperator<T>, IEnumerable<T>, IDisposable
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => _set.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _set.Keys.GetEnumerator();

        public int count => _set.Count;

        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private ObservationContext _context;
        private CollectionIdProvider _idProvider;
        private List<ObservableOperation<SetOpArgs<T>>> _initOperations = new List<ObservableOperation<SetOpArgs<T>>>();

        public SetObservable(params T[] source) : this(source, default) { }
        public SetObservable(ObservationContext context, params T[] source) : this(source, context) { }
        public SetObservable(IEnumerable<T> values, ObservationContext context = default) : this(context)
        {
            foreach (T value in values)
                _set.Add(value, _idProvider.GetUnusedId());
        }

        public SetObservable(ObservationContext context = default)
        {
            _context = context ?? ObservationContext.Default;
            _idProvider = new CollectionIdProvider(x => _set.ContainsValue(x));
        }

        void IObservable.InitializeObserver(IObserver observer)
        {
            while (_initOperations.Count > _set.Count)
                _initOperations.RemoveAt(_initOperations.Count - 1);

            while (_initOperations.Count < _set.Count)
                _initOperations.Add(new ObservableOperation<SetOpArgs<T>>() { source = this });

            var index = 0;
            foreach (var kvp in _set)
            {
                _initOperations[index].value = new SetOpArgs<T>(kvp.Value, kvp.Key, false);
                index++;
            }

            observer.OnNext(_initOperations);
        }

        IDisposable ISetOperator<T>.Subscribe(ISetObserver<T> observer)
            => _context.RegisterObserver(
                new Observer(
                    onNext: ops =>
                    {
                        foreach (var op in ops.Cast<IObservableOperation<SetOpArgs<T>>>())
                        {
                            if (op.value.isRemove)
                            {
                                observer.OnRemove(op.value.id, op.value.element);
                            }
                            else
                            {
                                observer.OnAdd(op.value.id, op.value.element);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                ),
                this
            );

        IDisposable ICollectionOperator<T>.Subscribe(ICollectionObserver<T> observer)
            => _context.RegisterObserver(
                new Observer(
                    onNext: ops =>
                    {
                        foreach (var op in ops.Cast<IObservableOperation<SetOpArgs<T>>>())
                        {
                            if (op.value.isRemove)
                            {
                                observer.OnRemove(op.value.id, op.value.element);
                            }
                            else
                            {
                                observer.OnAdd(op.value.id, op.value.element);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                ),
                this
            );

        public bool Add(T element)
        {
            if (_set.ContainsKey(element))
                return false;

            var id = _idProvider.GetUnusedId();
            _set.Add(element, id);
            _context.RegisterOperation(this, new SetOpArgs<T>(id, element, false));
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
            _context.RegisterOperation(this, new SetOpArgs<T>(id, element, true));

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                _context.RegisterOperation(this, new SetOpArgs<T>(kvp.Value, kvp.Key, true));
            }
        }

        public bool Contains(T element)
            => _set.ContainsKey(element);

        public void Dispose()
        {
            _context.HandleObservableDisposed(this);
        }
    }
}