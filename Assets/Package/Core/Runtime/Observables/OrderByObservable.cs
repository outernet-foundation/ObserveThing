using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class OrderByObservable<T, U> : IDisposable
    {
        private IListOperand<T> _operand;
        private Func<T, IObservable<U>> _orderBy;
        private Func<U, U, int> _compare;
        private Dictionary<uint, EntryData> _dataById = new Dictionary<uint, EntryData>();
        private List<EntryData> _order = new List<EntryData>();
        private IDisposable _subscriptions;

        private class EntryData
        {
            public uint id;
            public T element;
            public U orderBy;
            public IDisposable subscription;
        }

        public OrderByObservable(IObservable<CollectionOp<T>> source, Func<T, IObservable<U>> orderBy, bool descending, IListOperand<T> operand)
        {
            _orderBy = orderBy;
            _compare = descending ? DescendingCompare : AscendingCompare;
            _operand = operand;
            _subscriptions = source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _operand.OnError,
                onDispose: Dispose,
                immediate: true
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
                onError: _operand.OnError,
                immediate: true
            );
        }

        private void HandleRemove(uint id, T element)
        {
            var data = _dataById[id];
            int index = _order.IndexOf(data);
            _dataById.Remove(id);
            _order.RemoveAt(index);
            data.subscription.Dispose();
            _operand.RemoveAt(index);
        }

        private void Resort(EntryData data)
        {
            var originalIndex = _order.IndexOf(data);

            if (originalIndex != -1)
                _order.RemoveAt(originalIndex);

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

            if (originalIndex != newIndex)
            {
                if (originalIndex != -1)
                    _operand.RemoveAt(originalIndex);

                _operand.Insert(newIndex, data.element);
            }
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }
}
