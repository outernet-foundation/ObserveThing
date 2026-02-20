using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class OrderByDynamic<T, U> : IDisposable
    {
        private IDisposable _collectionStream;
        private Func<T, IValueObservable<U>> _orderBy;
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

        public OrderByDynamic(ICollectionObservable<T> collection, Func<T, IValueObservable<U>> orderBy, IListObserver<T> receiver)
        {
            _orderBy = orderBy;
            _receiver = receiver;

            _collectionStream = collection.SubscribeWithId(
                HandleAdd,
                HandleRemove,
                _receiver.OnError,
                Dispose
            );
        }

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
                onError: _receiver.OnError
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

                if (Comparer<U>.Default.Compare(data.orderBy, compareTo.orderBy) > 0)
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
