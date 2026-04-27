using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableDictionary<TKey, TValue> : ObservableDictionaryBase<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public int Count => GetCountInternal();

        public IEnumerable<TKey> Keys => GetKeysInternal();
        public IEnumerable<TValue> Values => GetValuesInternal();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => ElementsInternal().Select(x => KeyValuePair.Create(x.Key, x.Value.value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ElementsInternal().Select(x => KeyValuePair.Create(x.Key, x.Value.value)).GetEnumerator();

        public IEnumerable<(uint id, KeyValuePair<TKey, TValue> kvp)> ElementsWithIds
            => ElementsInternal().Select<KeyValuePair<TKey, (uint id, TValue value)>, (uint id, KeyValuePair<TKey, TValue>)>(x => new(x.Value.id, KeyValuePair.Create(x.Key, x.Value.value)));

        public TValue this[TKey key]
        {
            get => GetValue(key);
            set => SetInternal(key, value);
        }

        public ObservableDictionary(ObservationContext context, params KeyValuePair<TKey, TValue>[] source) : base(context, source) { }
        public ObservableDictionary(ObservationContext context, IEnumerable<KeyValuePair<TKey, TValue>> source) : base(context, source) { }
        public ObservableDictionary(ObservationContext context) : base(context, default) { }

        public ObservableDictionary(params KeyValuePair<TKey, TValue>[] source) : base(default, source) { }
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source) : base(default, source) { }
        public ObservableDictionary() : base(default, default) { }

        public void Add(TKey key, TValue value)
            => AddInternal(key, value);

        public bool Remove(TKey key)
            => RemoveInternal(key);

        public void Clear()
            => ClearInternal();

        public (uint id, TValue value) GetValueWithId(TKey key)
            => GetValueWithIdInternal(key);

        public bool TryGetValue(TKey key, out TValue value)
            => TryGetValueInternal(key, out value);

        public bool TryGetValueWithId(TKey key, out (uint id, TValue value) valueWithId)
            => TryGetValueWithIdInternal(key, out valueWithId);

        public bool ContainsKey(TKey key)
            => ContainsKeyInternal(key);

        public bool ContainsValue(TValue value)
            => ContainsValueInternal(value);
    }
}