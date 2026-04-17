using System;

namespace ObserveThing
{
    public class WithPreviousOperator<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<(T current, T previous)> _receiver;
        private T _previousValue;
        private bool _disposed;

        public WithPreviousOperator(IValueOperator<T> source, IValueObserver<(T current, T previous)> receiver)
        {
            _receiver = receiver;
            _sourceStream = source.Subscribe(
                onNext: HandleNext,
                onError: receiver.OnError,
                onDispose: Dispose,
                immediate: receiver.immediate
            );
        }

        private void HandleNext(T value)
        {
            var args = (value, _previousValue);
            _previousValue = value;
            _receiver.OnNext(args);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();
            _receiver.OnDispose();
        }
    }
}
