using System;

namespace ObserveThing
{
    public class CountOperator<T> : IDisposable
    {
        private ObservableValue<int> _operand;
        private IDisposable _subscriptions;

        public CountOperator(ICollectionObservable<T> source, ObservableValue<int> operand)
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
