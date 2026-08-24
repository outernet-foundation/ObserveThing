using System;

namespace ObserveThing
{
    public class CountObservable<T> : IDisposable
    {
        private ObservableValue<int> _operand;
        private IDisposable _subscriptions;

        public CountObservable(ICollectionObservable<T> source, ObservableValue<int> operand)
        {
            _operand = operand;
            _subscriptions = source.Subscribe(
                onAdd: _ => operand.value = operand.value + 1,
                onRemove: _ => operand.value = operand.value - 1,
                onError: operand.OnError,
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
