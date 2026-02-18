using System;

namespace ObserveThing
{
    public class ValueShallowCopyDynamic<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<T> _receiver;
        private IDisposable _nestedSubscription = default;
        private bool _changingNestedSource = false;
        private ValueObserver<T> _nestedObserver;
        private bool _disposed;

        public ValueShallowCopyDynamic(IValueObservable<IValueObservable<T>> source, IValueObserver<T> receiver)
        {
            _receiver = receiver;

            _nestedObserver = new ValueObserver<T>(
                onNext: _receiver.OnNext,
                onError: _receiver.OnError,
                onDispose: () =>
                {
                    if (!_changingNestedSource)
                        receiver.OnNext(default);
                }
            );

            _sourceStream = source.Subscribe(
                onNext: HandleNext,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleNext(IValueObservable<T> value)
        {
            _changingNestedSource = true;
            _nestedSubscription?.Dispose();
            _changingNestedSource = false;

            if (_nestedObserver == null)
            {
                _nestedSubscription = null;
                _receiver.OnNext(default);
                return;
            }

            _nestedSubscription = value?.Subscribe(_nestedObserver);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();
            _nestedSubscription?.Dispose();

            _receiver.OnDispose();
        }
    }
}