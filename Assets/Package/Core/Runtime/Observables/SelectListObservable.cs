using System;

namespace ObserveThing
{
    public class SelectListObservable<T, U> : IDisposable
    {
        private ObservableList<U> _operand;
        private IDisposable _subscriptions;

        public SelectListObservable(IListObservable<T> source, Func<T, U> select, ObservableList<U> operand)
        {
            _operand = operand;
            _subscriptions = source.Subscribe(
                onAdd: (index, value) => _operand.Insert(index, select(value)),
                onRemove: (index, _) => _operand.RemoveAt(index),
                onError: _operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }
}