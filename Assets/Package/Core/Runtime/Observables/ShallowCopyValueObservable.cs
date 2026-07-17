using System;

namespace ObserveThing
{
    public class ShallowCopyValueObservable<T> : IDisposable
    {
        private IValueOperand<T> _operand;
        private Observer<T> _nestedObserver;
        private IDisposable _nestedSubscription;
        private IDisposable _subscriptions;

        public ShallowCopyValueObservable(IObservable<IObservable<T>> source, IValueOperand<T> operand)
        {
            _operand = operand;

            _nestedObserver = new Observer<T>(
                onNext: x => operand.value = x,
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

        private void HandleNext(IObservable<T> value)
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