using System;

namespace ObserveThing
{
    public class SelectValueOperator<T, U> : IDisposable
    {
        private ObservableValue<U> _operand;
        private IDisposable _subscriptions;

        public SelectValueOperator(IValueObservable<T> source, Func<T, U> select, ObservableValue<U> operand)
        {
            _operand = operand;
            _subscriptions = source.Subscribe(
                onNext: x => operand.value = select(x),
                onError: operand.OnError,
                onDispose: operand.Dispose,
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