using System;

namespace ObserveThing
{
    public class ShallowCopyValueObservable<T> : IDisposable
    {
        private IValueOperand<T> _operand;
        private Observer<IValueOperation<T>> _nestedObserver;
        private IDisposable _nestedSubscription;
        private IDisposable _subscriptions;

        public ShallowCopyValueObservable(IValueObservable<IValueObservable<T>> source, IValueOperand<T> operand)
        {
            _operand = operand;

            _nestedObserver = new Observer<IValueOperation<T>>(
                onNext: x => operand.value = x.value,
                onError: operand.OnError,
                immediate: true
            );

            _subscriptions = source.Subscribe(
                onNext: HandleNext,
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        private void HandleNext(IValueObservable<T> value)
        {
            _nestedSubscription?.Dispose();
            _nestedSubscription = null;

            if (value == null)
            {
                _operand.value = default;
                return;
            }

            _nestedSubscription = value.Subscribe(_nestedObserver);
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            _nestedSubscription?.Dispose();
            _nestedSubscription = null;

            _operand.OnDisposed();
        }
    }
}