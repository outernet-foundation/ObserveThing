using System;

namespace ObserveThing
{
    public class SkipWhileObservable<T> : IDisposable
    {
        private ObservableValue<T> _operand;
        private IDisposable _subscriptions;

        public SkipWhileObservable(IValueObservable<T> source, Func<bool> skipWhile, ObservableValue<T> operand)
        {
            _operand = operand;
            _subscriptions = source.Subscribe(
                onNext: value =>
                {
                    if (!skipWhile())
                        _operand.value = value;
                },
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