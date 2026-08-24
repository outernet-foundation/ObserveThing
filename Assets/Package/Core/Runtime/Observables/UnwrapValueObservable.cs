using System;

namespace ObserveThing
{
    public class UnwrapValueObservable<T> : IDisposable
    {
        private ObservableValue<T> _operand;
        private ValueObserver<T> _nestedObserver;
        private IDisposable _nestedSubscription;
        private IDisposable _subscriptions;

        public UnwrapValueObservable(IValueObservable<IValueObservable<T>> source, ObservableValue<T> operand)
        {
            _operand = operand;

            _nestedObserver = new ValueObserver<T>(
                onNext: x => operand.value = x,
                onError: operand.OnError
            );

            _subscriptions = source.Subscribe(
                new ValueObserver<IValueObservable<T>>(
                    onNext: HandleNext,
                    onDispose: Dispose,
                    onError: operand.OnError
                ),
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

            _nestedSubscription = value.Subscribe(_nestedObserver, immediate: true);
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            _nestedSubscription?.Dispose();
            _nestedSubscription = null;

            _operand.Dispose();
        }
    }
}