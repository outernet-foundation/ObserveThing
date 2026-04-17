using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class OrderByObservable<T, U> : IDisposable
    {
        private IDisposable _collectionStream;
        private Func<T, IValueOperator<U>> _orderBy;
        private Func<U, U, int> _compare;
        private IListObserver<T> _receiver;
        private Dictionary<uint, EntryData> _dataById = new Dictionary<uint, EntryData>();
        private List<EntryData> _order = new List<EntryData>();
        private bool _disposed = false;

        private class EntryData
        {
            public uint id;
            public T element;
            public U orderBy;
            public IDisposable subscription;
        }

        public OrderByObservable(ICollectionOperator<T> collection, Func<T, IValueOperator<U>> orderBy, bool descending, IListObserver<T> receiver)
        {
            _orderBy = orderBy;
            _compare = descending ? DescendingCompare : AscendingCompare;
            _receiver = receiver;

            _collectionStream = collection.SubscribeWithId(
                HandleAdd,
                HandleRemove,
                _receiver.OnError,
                Dispose,
                immediate: receiver.immediate
            );
        }

        private int DescendingCompare(U v1, U v2)
            => Comparer<U>.Default.Compare(v2, v1);

        private int AscendingCompare(U v1, U v2)
            => Comparer<U>.Default.Compare(v1, v2);

        private void HandleAdd(uint id, T element)
        {
            var data = new EntryData()
            {
                id = id,
                element = element
            };

            _dataById.Add(id, data);
            data.subscription = _orderBy(element).Subscribe(
                onNext: x =>
                {
                    data.orderBy = x;
                    Resort(data);
                },
                onError: _receiver.OnError,
                immediate: _receiver.immediate
            );
        }

        private void HandleRemove(uint id, T element)
        {
            var data = _dataById[id];
            int index = _order.IndexOf(data);
            _dataById.Remove(id);
            _order.RemoveAt(index);
            data.subscription.Dispose();
            _receiver.OnRemove(id, index, data.element);
        }

        private void Resort(EntryData data)
        {
            var originalIndex = _order.IndexOf(data);

            if (originalIndex != -1)
            {
                _order.RemoveAt(originalIndex);
                _receiver.OnRemove(data.id, originalIndex, data.element);
            }

            int newIndex = -1;

            for (int i = 0; i < _order.Count; i++)
            {
                var compareTo = _order[i];

                if (_compare(data.orderBy, compareTo.orderBy) > 0)
                    continue;

                newIndex = i;
                break;
            }

            if (newIndex == -1)
                newIndex = _order.Count;

            _order.Insert(newIndex, data);

            _receiver.OnAdd(data.id, newIndex, data.element);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _collectionStream.Dispose();

            foreach (var entry in _order)
                entry.subscription.Dispose();

            _receiver.OnDispose();
        }
    }
}
