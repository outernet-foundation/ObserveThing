using System;

namespace ObserveThing
{
    public class SkipWhileObservable<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<T> _receiver;
        private Func<bool> _skipWhile;
        private bool _disposed;

        public SkipWhileObservable(IValueOperator<T> source, Func<bool> skipWhile, IValueObserver<T> receiver)
        {
            _receiver = receiver;
            _skipWhile = skipWhile;

            _sourceStream = source.Subscribe(
                onNext: HandleSourceChanged,
                onError: receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleSourceChanged(T value)
        {
            if (!_skipWhile())
                _receiver.OnNext(value);
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