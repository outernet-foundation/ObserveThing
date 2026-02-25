using System;

namespace ObserveThing
{
    public class ShallowCopyValueObservable<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<T> _receiver;
        private IDisposable _nestedSubscription = default;
        private bool _changingNestedSource = false;
        private ValueObserver<T> _nestedObserver;
        private T _latest;
        private bool _disposed;

        public ShallowCopyValueObservable(IValueObservable<IValueObservable<T>> source, IValueObserver<T> receiver)
        {
            _receiver = receiver;

            _nestedObserver = new ValueObserver<T>(
                onNext: x =>
                {
                    if (!Equals(x, _latest))
                    {
                        _latest = x;
                        _receiver.OnNext(x);
                    }
                },
                onError: _receiver.OnError,
                onDispose: () =>
                {
                    if (!_changingNestedSource && !_disposed)
                    {
                        _latest = default;
                        _receiver.OnNext(default);
                    }
                }
            );

            _sourceStream = source.Subscribe(
                onNext: HandleNext,
                onError: _receiver.OnError,
                onDispose: Dispose
            );

            if (Equals(_latest, default(T)))
                _receiver.OnNext(default);
        }

        private void HandleNext(IValueObservable<T> value)
        {
            _changingNestedSource = true;
            _nestedSubscription?.Dispose();
            _changingNestedSource = false;

            if (value == null)
            {
                _nestedSubscription = null;

                if (!Equals(_latest, default(T)))
                {
                    _latest = default;
                    _receiver.OnNext(default);
                }

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