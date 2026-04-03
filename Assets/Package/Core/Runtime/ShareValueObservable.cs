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

        public ShareValueObservable(IValueObservable<T> source, SynchronizationContext context = default)
        {
            _source = source;
            _shared = new ValueObservable<T>(context);
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

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ValueObserver<T>(
                onNext: _ => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

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