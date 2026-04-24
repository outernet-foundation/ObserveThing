using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class ObservableReadOnlyCollection<T> : ObservableCollectionBase<T>, IEnumerable<T>
    {
        public int count => GetCountInternal();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetElementsWithIdsInternal().Select(x => x.element).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetElementsWithIdsInternal().Select(x => x.element).GetEnumerator();

        public IEnumerable<(uint id, T element)> ElementsWithIds
            => GetElementsWithIdsInternal();

        public ObservableReadOnlyCollection(IEnumerable<T> value) : this(default, value) { }

        public ObservableReadOnlyCollection(ObservationContext context, IEnumerable<T> value) : base(context)
        {
            uint nextId = 1;
            foreach (var element in value)
            {
                AddInternal(nextId, element);
                nextId++;
            }
        }
    }
}