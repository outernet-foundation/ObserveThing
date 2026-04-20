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

    public class DictionaryObservable<TKey, TValue> : IOperationObservable, IDictionaryObservable<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
    {
        public TValue this[TKey key]
        {
            get => _dictionary[key].value;
            set
            {
                if (!_dictionary.TryGetValue(key, out var prevValue) || Equals(value, prevValue.value))
                    return;

                Remove(key);
                Add(key, value);
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => _dictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _dictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.value)).GetEnumerator();

        public IEnumerable<TKey> keys => _dictionary.Keys;
        public IEnumerable<TValue> values => _dictionary.Values.Select(x => x.value);

        public int count => _dictionary.Count;

        private Dictionary<TKey, (uint id, TValue value)> _dictionary = new Dictionary<TKey, (uint id, TValue value)>();
        private ObservationContext _context;
        private CollectionIdProvider _idProvider;

        public DictionaryObservable(params KeyValuePair<TKey, TValue>[] source) : this(source, default) { }
        public DictionaryObservable(ObservationContext context, params KeyValuePair<TKey, TValue>[] source) : this(source, context) { }
        public DictionaryObservable(IEnumerable<KeyValuePair<TKey, TValue>> source, ObservationContext context = default) : this(context)
        {
            foreach (var kvp in source)
                _dictionary.Add(kvp.Key, new(_idProvider.GetUnusedId(), kvp.Value));
        }

        public DictionaryObservable(ObservationContext context = default)
        {
            _context = context ?? ObservationContext.Default;
            _idProvider = new CollectionIdProvider(x => _dictionary.Values.Any(y => y.id == x));
        }

        IDisposable IDictionaryObservable<TKey, TValue>.Subscribe(IDictionaryObserver<TKey, TValue> observer)
            => _context.RegisterObserver(
                new OperationObserver(
                    onNext: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            foreach (var kvp in _dictionary)
                                observer.OnAdd(kvp.Value.id, new(kvp.Key, kvp.Value.value));

                            return;
                        }

                        foreach (var op in ops.Cast<IOperation<DictionaryOpArgs<TKey, TValue>>>())
                        {
                            if (op.value.isRemove)
                            {
                                observer.OnRemove(op.value.id, op.value.kvp);
                            }
                            else
                            {
                                observer.OnAdd(op.value.id, op.value.kvp);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                ),
                this
            );

        IDisposable ICollectionObservable<KeyValuePair<TKey, TValue>>.Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer)
            => _context.RegisterObserver(
                new OperationObserver(
                    onNext: ops =>
                    {
                        //init
                        if (ops == null)
                        {
                            foreach (var kvp in _dictionary)
                                observer.OnAdd(kvp.Value.id, new(kvp.Key, kvp.Value.value));

                            return;
                        }

                        foreach (var op in ops.Cast<IOperation<DictionaryOpArgs<TKey, TValue>>>())
                        {
                            if (op.value.isRemove)
                            {
                                observer.OnRemove(op.value.id, op.value.kvp);
                            }
                            else
                            {
                                observer.OnAdd(op.value.id, op.value.kvp);
                            }
                        }
                    },
                    onError: observer.OnError,
                    onDispose: observer.OnDispose,
                    immediate: observer.immediate
                ),
                this
            );

        public IDisposable Subscribe(IOperationObserver observer)
            => _context.RegisterObserver(observer, this);

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out var data))
            {
                value = data.value;
                return true;
            }

            value = default;
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            var id = _idProvider.GetUnusedId();
            _dictionary.Add(key, (id, value));
            _context.RegisterOperation(this, new DictionaryOpArgs<TKey, TValue>(id, new(key, value), false));
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var data))
                return false;

            _dictionary.Remove(key);
            _context.RegisterOperation(this, new DictionaryOpArgs<TKey, TValue>(data.id, new(key, data.value), true));

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                _context.RegisterOperation(this, new DictionaryOpArgs<TKey, TValue>(kvp.Value.id, new(kvp.Key, kvp.Value.value), true));
            }
        }

        public bool ContainsKey(TKey key)
            => _dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value)
            => _dictionary.Values.Select(x => x.value).Contains(value);

        public void Dispose()
        {
            _context.HandleObservableDisposed(this);
        }
    }
}