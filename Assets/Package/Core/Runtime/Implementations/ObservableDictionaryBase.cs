using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct DictionaryOpArgs<TKey, TValue>
    {
        public KeyValuePair<TKey, TValue> kvp;
        public bool isRemove;
        public uint elementId;
    }

    public class ObservableDictionaryBase<TKey, TValue> : ObservableBase<IDictionaryObserver<TKey, TValue>, DictionaryOpArgs<TKey, TValue>>, IDictionaryObservable<TKey, TValue>
    {
        private Dictionary<TKey, (uint id, TValue value)> _dictionary = new Dictionary<TKey, (uint id, TValue value)>();
        private CollectionIdProvider _idProvider;
        private Stack<Operation> _operationPool = new Stack<Operation>();

        public ObservableDictionaryBase(ObservationContext context) : this(context, null) { }
        public ObservableDictionaryBase(ObservationContext context, IEnumerable<KeyValuePair<TKey, TValue>> value) : base(context)
        {
            _idProvider = new CollectionIdProvider(x => _dictionary.Values.Any(y => y.id == x));

            if (value == null)
                return;

            foreach (var kvp in value)
                _dictionary.Add(kvp.Key, new(_idProvider.GetUnusedId(), kvp.Value));
        }

        protected int GetCountInternal()
            => _dictionary.Count;

        protected IEnumerable<TKey> GetKeysInternal()
            => _dictionary.Keys;

        protected IEnumerable<TValue> GetValuesInternal()
            => _dictionary.Values.Select(x => x.value);

        protected IEnumerable<KeyValuePair<TKey, (uint id, TValue value)>> ElementsInternal()
            => _dictionary;

        protected void SetInternal(TKey key, TValue value)
        {
            RemoveInternal(key);
            AddInternal(key, value);
        }

        protected void AddInternal(TKey key, TValue value)
        {
            var id = _idProvider.GetUnusedId();
            _dictionary.Add(key, (id, value));
            EnqueuePendingOperation(new DictionaryOpArgs<TKey, TValue>() { elementId = id, kvp = KeyValuePair.Create(key, value), isRemove = false });
        }

        protected bool RemoveInternal(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            EnqueuePendingOperation(new DictionaryOpArgs<TKey, TValue>() { elementId = data.id, kvp = KeyValuePair.Create(key, data.value), isRemove = true });

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                EnqueuePendingOperation(new DictionaryOpArgs<TKey, TValue>() { elementId = kvp.Value.id, kvp = KeyValuePair.Create(kvp.Key, kvp.Value.value), isRemove = true });
            }
        }

        public TValue GetValue(TKey key)
            => _dictionary[key].value;

        protected (uint id, TValue value) GetValueWithIdInternal(TKey key)
            => _dictionary[key];

        protected bool TryGetValueInternal(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out var data))
            {
                value = data.value;
                return true;
            }

            value = default;
            return false;
        }

        protected bool TryGetValueWithIdInternal(TKey key, out (uint id, TValue value) valueWithId)
            => _dictionary.TryGetValue(key, out valueWithId);

        protected bool ContainsKeyInternal(TKey key)
            => _dictionary.ContainsKey(key);

        protected bool ContainsValueInternal(TValue value)
            => _dictionary.Values.Select(x => x.value).Contains(value);

        protected override void SendOperation(IDictionaryObserver<TKey, TValue> observer, DictionaryOpArgs<TKey, TValue> operation)
        {
            if (operation.isRemove)
            {
                observer.OnRemove(operation.elementId, operation.kvp);
            }
            else
            {
                observer.OnAdd(operation.elementId, operation.kvp);
            }
        }

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer, bool immediate = false, uint? priority = null)
        {
            var subscription = AddObserver(observer, immediate, priority);

            foreach (var kvp in _dictionary)
                observer.OnAdd(kvp.Value.id, KeyValuePair.Create(kvp.Key, kvp.Value.value));

            return subscription;
        }

        public IDisposable Subscribe(IObserver observer, bool immediate = false, uint? priority = null)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, kvp) =>
                {
                    var operation = _operationPool.TryPop(out var op) ? op : new Operation(this);
                    operation.args = new DictionaryOpArgs<TKey, TValue>() { elementId = id, kvp = kvp, isRemove = false };
                    observer.OnNext(operation);
                    _operationPool.Push(operation);
                },
                onRemove: (id, kvp) =>
                {
                    var operation = _operationPool.TryPop(out var op) ? op : new Operation(this);
                    operation.args = new DictionaryOpArgs<TKey, TValue>() { elementId = id, kvp = kvp, isRemove = true };
                    observer.OnNext(operation);
                    _operationPool.Push(operation);
                },
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable IDictionaryObservable.Subscribe(IDictionaryObserver observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, kvp) => observer.OnAdd(id, new KeyValuePair<object, object>(kvp.Key, kvp.Value)),
                onRemove: (id, kvp) => observer.OnRemove(id, new KeyValuePair<object, object>(kvp.Key, kvp.Value)),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable<KeyValuePair<TKey, TValue>>.Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, kvp) => observer.OnAdd(id, kvp),
                onRemove: (id, kvp) => observer.OnRemove(id, kvp),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, kvp) => observer.OnAdd(id, kvp),
                onRemove: (id, kvp) => observer.OnRemove(id, kvp),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}