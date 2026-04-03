using System;

namespace ObserveThing
{
    public class ThenObservable<T> : IDisposable
    {
        private IDisposable _sourceStream;
        public IValueObserver<T> _thenReceiver;
        private IValueObserver<T> _receiver;
        private bool _initialized;
        private bool _disposed;

        public ThenObservable(IValueObservable<T> source, IValueObserver<T> thenReceiver, IValueObserver<T> receiver)
        {
            _thenReceiver = thenReceiver;
            _receiver = receiver;
            _sourceStream = source.Subscribe(
                onNext: HandleNext,
                onError: _receiver.OnError,
                onDispose: Dispose,
                immediate: receiver.immediate
            );

            // Always send init call
            if (!_initialized)
                HandleNext(default);
        }

        private void HandleNext(T value)
        {
            _initialized = true;
            _thenReceiver.OnNext(value);
            _receiver.OnNext(value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _thenReceiver.OnDispose();
            _receiver.OnDispose();
        }
    }
}