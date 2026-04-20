using System;

namespace ObserveThing
{
    public class ShareValueObservable<T> : IValueObservable<T>
    {
        private IValueObservable<T> _source;
        private IDisposable _sourceStream;
        private ValueObservable<T> _shared;
        private int _observerCount;
        private bool _disposed;

        public ShareValueObservable(IValueObservable<T> source)
        {
            _source = source;
            _shared = new ValueObservable<T>(new ObservationContext());
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
        {
            _observerCount++;

            if (_observerCount == 1)
            {
                _sourceStream = _source.Subscribe(
                    immediate: true,
                    onNext: value => _shared.value = value
                );
            }

            return _shared.Subscribe(
                immediate: observer.immediate,
                onNext: observer.OnNext,
                onError: observer.OnError,
                onDispose: () =>
                {
                    observer.OnDispose();

                    _observerCount--;
                    if (_observerCount == 0)
                    {
                        _sourceStream.Dispose();
                        _sourceStream = null;
                        _shared.value = default;
                    }
                }
            );
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream?.Dispose();
            _shared.Dispose();
        }
    }
}