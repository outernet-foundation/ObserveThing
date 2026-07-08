using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class DistinctObservable<T> : IDisposable
    {
        private ISetOperand<T> _operand;
        private Dictionary<T, int> _countByElement = new Dictionary<T, int>();
        private IDisposable _subscriptions;

        public DistinctObservable(ICollectionObservable<T> source, ISetOperand<T> operand)
        {
            _operand = operand;
            _subscriptions = source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        private void HandleAdd(T value)
        {
            bool isNew = !_countByElement.TryGetValue(value, out var count);
            _countByElement[value] = count + 1;

            if (isNew)
                _operand.Add(value);
        }

        private void HandleRemove(T value)
        {
            var count = _countByElement[value];

            if (count > 1)
            {
                _countByElement[value] = count - 1;
            }
            else
            {
                _countByElement.Remove(value);
                _operand.Remove(value);
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