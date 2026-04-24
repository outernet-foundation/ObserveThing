using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public struct DictionaryOpArgs<TKey, TValue>
    {
        public uint id { get; }
        public KeyValuePair<TKey, TValue> kvp { get; }
        public bool isRemove { get; }

        public DictionaryOpArgs(uint id, KeyValuePair<TKey, TValue> kvp, bool isRemove)
        {
            this.id = id;
            this.kvp = kvp;
            this.isRemove = isRemove;
        }
    }

    public class ObservableDictionaryBase<TKey, TValue> : Observable<DictionaryOpArgs<TKey, TValue>>, IDictionaryObservable<TKey, TValue>
    {
        private Dictionary<TKey, (uint id, TValue value)> _dictionary = new Dictionary<TKey, (uint id, TValue value)>();
        private CollectionIdProvider _idProvider;
        private List<DictionaryOpArgs<TKey, TValue>> _initOps = new List<DictionaryOpArgs<TKey, TValue>>();

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

        protected override IReadOnlyList<DictionaryOpArgs<TKey, TValue>> GetInitializationOperations()
        {
            _initOps.Clear();
            _initOps.AddRange(_dictionary.Select(x => new DictionaryOpArgs<TKey, TValue>(x.Value.id, KeyValuePair.Create(x.Key, x.Value.value), false)));
            return _initOps;
        }

        protected void SetInternal(TKey key, TValue value)
        {
            RemoveInternal(key);
            AddInternal(key, value);
        }

        protected void AddInternal(TKey key, TValue value)
        {
            var id = _idProvider.GetUnusedId();
            _dictionary.Add(key, (id, value));
            EnqueuePendingOperation(new DictionaryOpArgs<TKey, TValue>(id, new(key, value), false));
        }

        protected bool RemoveInternal(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            EnqueuePendingOperation(new DictionaryOpArgs<TKey, TValue>(data.id, new(key, data.value), true));

            return true;
        }

        protected void ClearInternal()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                EnqueuePendingOperation(new DictionaryOpArgs<TKey, TValue>(kvp.Value.id, new(kvp.Key, kvp.Value.value), true));
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

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer)
            => Subscribe(
                new Observer<DictionaryOpArgs<TKey, TValue>>(
                    onOperation: ops =>
                    {
                        foreach (var op in ops)
                        {
                            if (op.isRemove)
                            {
                                observer.OnRemove(op.id, op.kvp);
                            }
                            else
                            {
                                observer.OnAdd(op.id, op.kvp);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                )
            );

        public IDisposable Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer)
            => Subscribe(
                new Observer<DictionaryOpArgs<TKey, TValue>>(
                    onOperation: ops =>
                    {
                        foreach (var op in ops)
                        {
                            if (op.isRemove)
                            {
                                observer.OnRemove(op.id, op.kvp);
                            }
                            else
                            {
                                observer.OnAdd(op.id, op.kvp);
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