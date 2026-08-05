using System;

namespace ObserveThing
{
    public class SelectValueOperator<T, U> : IDisposable
    {
        private IValueOperand<U> _operand;
        private IDisposable _subscriptions;

        public SelectValueOperator(IValueObservable<T> source, Func<T, U> select, IValueOperand<U> operand)
        {
            _operand = operand;
            _subscriptions = source.Subscribe(
                onNext: x => operand.value = select(x),
                onError: operand.OnError,
                onDispose: operand.OnDisposed,
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