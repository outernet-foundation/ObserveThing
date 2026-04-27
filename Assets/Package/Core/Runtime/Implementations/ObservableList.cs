using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableList<T> : ObservableListBase<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => ElementsInternal().Select(x => x.value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ElementsInternal().Select(x => x.value).GetEnumerator();

        public IEnumerable<(uint id, T element)> ElementsWithIds
            => ElementsInternal();

        public int Count => GetCountInternal();

        public T this[int index]
        {
            get => ElementAt(index);
            set
            {
                if (Equals(ElementAt(index), value))
                    return;

                RemoveAt(index);
                Insert(index, value);
            }
        }

        public ObservableList(ObservationContext context, params T[] value) : base(context, value) { }
        public ObservableList(ObservationContext context, IEnumerable<T> value) : base(context, value) { }
        public ObservableList(ObservationContext context) : base(context, default) { }

        public ObservableList(params T[] value) : base(default, value) { }
        public ObservableList(IEnumerable<T> value) : base(default, value) { }
        public ObservableList() : base(default, default) { }

        public void Add(T added)
            => AddInternal(added);

        public void AddRange(IEnumerable<T> toAdd)
            => AddRangeInternal(toAdd);

        public bool Remove(T removed)
            => RemoveInternal(removed);

        public void RemoveAt(int index)
            => RemoveAtInternal(index);

        public void Insert(int index, T item)
            => InsertInternal(index, item);

        public void Clear()
            => ClearInternal();

        public T ElementAt(int index)
            => ElementAtInternal(index);

        public (uint id, T value) ElementAndIdAt(int index)
            => ElementAndIdAtInternal(index);

        public int IndexOf(T item)
            => IndexOfInternal(item);

        public bool Contains(T item)
            => ContainsInternal(item);
    }
}