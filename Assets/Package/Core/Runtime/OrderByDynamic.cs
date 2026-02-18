using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class OrderByDynamic<T, U> : IDisposable
    {
        private IDisposable _collectionStream;
        private Func<T, IValueObservable<U>> _orderBy;
        private IListObserver<T> _receiver;
        private Dictionary<T, EntryData> _dataByElement = new Dictionary<T, EntryData>();
        private List<EntryData> _order = new List<EntryData>();
        private bool _disposed = false;

        private class EntryData
        {
            public T element;
            public U orderBy;
            public IDisposable orderByStream;
            public int count;
        }

        public OrderByDynamic(ICollectionObservable<T> collection, Func<T, IValueObservable<U>> orderBy, IListObserver<T> receiver)
        {
            _orderBy = orderBy;
            _receiver = receiver;

            _collectionStream = collection.Subscribe(
                HandleAdd,
                HandlRemove,
                _receiver.OnError,
                Dispose
            );
        }

        private void HandleAdd(T element)
        {
            if (!_dataByElement.TryGetValue(element, out var data))
            {
                data = new EntryData() { element = element };
                _dataByElement.Add(element, data);
                data.orderByStream = _orderBy(element).Subscribe(
                    onNext: x =>
                    {
                        data.orderBy = x;
                        Resort(data);
                    },
                    onError: _receiver.OnError
                );
            }

            data.count++;
            _receiver.OnAdd(GetCurrentIndex(data), data.element);
        }

        private void HandlRemove(T element)
        {
            var data = _dataByElement[element];
            int index = GetCurrentIndex(data);

            data.count--;

            if (data.count == 0)
            {
                _dataByElement.Remove(element);
                _order.Remove(data);
            }

            _receiver.OnRemove(index, data.element);
        }

        private int GetCurrentIndex(EntryData data)
        {
            int index = 0;

            foreach (var entry in _order)
            {
                if (entry == data)
                    return index;

                index += entry.count;
            }

            throw new Exception($"Data for element {data.element} not found.");
        }

        private void Resort(EntryData data)
        {
            var originalIndex = GetCurrentIndex(data);

            for (int i = 0; i < data.count; i++)
                _receiver.OnRemove(originalIndex, data.element);

            _order.Remove(data);

            int newIndex = 0;

            for (int i = 0; i < _order.Count; i++)
            {
                var compareTo = _order[i];
                if (Comparer<U>.Default.Compare(data.orderBy, compareTo.orderBy) > 0)
                {
                    newIndex += compareTo.count;
                }
                else
                {
                    _order.Insert(i, data);
                    break;
                }
            }

            for (int i = 0; i < data.count; i++)
                _receiver.OnAdd(i, data.element);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _collectionStream.Dispose();

            foreach (var entry in _order)
                entry.orderByStream.Dispose();

            _receiver.OnDispose();
        }
    }
}
