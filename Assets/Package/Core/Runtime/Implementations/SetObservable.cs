using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class SetObservable<T> : ObservableBase<ISetObserver<T>>, ISetObservable<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => _set.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _set.Keys.GetEnumerator();

        public int count => _set.Count;

        private Dictionary<T, uint> _set = new Dictionary<T, uint>();
        private CollectionIdProvider _idProvider;

        public SetObservable(params T[] source) : this(source, default) { }
        public SetObservable(SynchronizationContext context, params T[] source) : this(source, context) { }
        public SetObservable(IEnumerable<T> values, SynchronizationContext context = default) : this(context)
        {
            foreach (T value in values)
                _set.Add(value, _idProvider.GetUnusedId());
        }

        public SetObservable(SynchronizationContext context = default) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _set.ContainsValue(x));
        }

        public bool Add(T element)
        {
            if (_set.ContainsKey(element))
                return false;

            var id = _idProvider.GetUnusedId();
            _set.Add(element, id);
            EnqueueNotify(x => x.OnAdd(id, element));
            return true;
        }

        public bool Remove(T element)
        {
            if (!_set.TryGetValue(element, out var id))
                return false;

            _set.Remove(element);
            EnqueueNotify(x => x.OnRemove(id, element));

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _set.ToArray())
            {
                _set.Remove(kvp.Key);
                EnqueueNotify(x => x.OnRemove(kvp.Value, kvp.Key));
            }
        }

        public bool Contains(T element)
            => _set.ContainsKey(element);

        public IDisposable Subscribe(ISetObserver<T> observer)
        {
            var subscription = AddObserver(observer);

            foreach (var kvp in _set)
                observer.OnAdd(kvp.Value, kvp.Key);

            return subscription;
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new SetObserver<T>(
                onAdd: observer.OnAdd,
                onRemove: observer.OnRemove,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new SetObserver<T>(
                onAdd: (_, _) => observer.OnChange(),
                onRemove: (_, _) => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }
}