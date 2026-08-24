using System;

namespace ObserveThing
{
    public class WithPreviousObservable<T> : IDisposable
    {
        private ObservableValue<(T previous, T current)> _operand;
        private T _previousValue;
        private IDisposable _subscriptions;

        public WithPreviousObservable(IValueObservable<T> source, ObservableValue<(T previous, T current)> operand)
        {
            _operand = operand;
            _subscriptions = source.Subscribe(
                onNext: HandleNext,
                onError: _operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        private void HandleNext(T value)
        {
            var pair = (value, _previousValue);
            _previousValue = value;
            _operand.value = pair;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }
}
