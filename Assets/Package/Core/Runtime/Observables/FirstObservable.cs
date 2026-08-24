using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class FirstObservable<T> : IDisposable
    {
        private ObservableValue<(bool found, T value)> _operand;
        private List<(uint id, T value)> _sortedList = new List<(uint id, T value)>();
        private (uint id, T value) _latest;
        private IDisposable _subscriptions;

        public FirstObservable(ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate, ObservableValue<(bool found, T value)> operand)
        {
            _operand = operand;
            _subscriptions = source.ObservableWhere(x => validate(x)).SubscribeWithId(
                onAdd: (id, value) =>
                {
                    _sortedList.Add(new(id, value));
                    _sortedList.Sort((x, y) => x.id.CompareTo(y.id));
                    SetValueIfNecessary();
                },
                onRemove: (id, value) =>
                {
                    _sortedList.Remove(new(id, value));
                    SetValueIfNecessary();
                },
                onError: operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        private void SetValueIfNecessary()
        {
            bool found = _sortedList.Count != 0;
            var next = _sortedList.Count == 0 ? new(0, default) : _sortedList[0];

            if (_latest.id == next.id && Equals(_latest.value, next.value))
                return;

            _latest = next;
            _operand.value = new(found, _latest.value);
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }
}