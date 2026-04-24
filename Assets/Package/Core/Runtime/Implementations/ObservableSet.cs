using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableSet<T> : ObservableSetBase<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetElementsInternal().Select(x => x.Key).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetElementsInternal().Select(x => x.Key).GetEnumerator();

        public IEnumerable<(uint id, T element)> ElementsWithIds
            => GetElementsInternal().Select<KeyValuePair<T, uint>, (uint id, T element)>(x => new(x.Value, x.Key));

        public int Count => GetCountInternal();

        public ObservableSet(ObservationContext context, params T[] source) : base(context, source) { }
        public ObservableSet(ObservationContext context, IEnumerable<T> source) : base(context, source) { }
        public ObservableSet(ObservationContext context) : base(context, default) { }

        public ObservableSet(params T[] source) : base(default, source) { }
        public ObservableSet(IEnumerable<T> source) : base(default, source) { }
        public ObservableSet() : base(default, default) { }

        public bool Add(T element)
            => AddInternal(element);

        public void AddRange(IEnumerable<T> elements)
            => AddRangeInternal(elements);

        public bool Remove(T element)
            => RemoveInternal(element);

        public void Clear()
            => ClearInternal();

        public bool Contains(T element)
            => ContainsInternal(element);
    }
}