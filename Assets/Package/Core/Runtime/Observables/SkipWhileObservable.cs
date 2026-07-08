using System;

namespace ObserveThing
{
    public class SkipWhileObservable<T> : IDisposable
    {
        private IValueOperand<T> _operand;
        private IDisposable _subscriptions;

        public SkipWhileObservable(IValueObservable<T> source, Func<bool> skipWhile, IValueOperand<T> operand)
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
            _operand.OnDisposed();
        }
    }
}