using System;

namespace ObserveThing
{
    public class ValueSelectDynamic<T, U> : IDisposable
    {
        private IDisposable _sourceStream;
        private Func<T, U> _select;
        private IValueObserver<U> _receiver;
        private bool _disposed;

        public ValueSelectDynamic(IValueObservable<T> source, Func<T, U> select, IValueObserver<U> receiver)
        {
            _receiver = receiver;
            _select = select;
            _sourceStream = source.Subscribe(
                onNext: HandleNext,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleNext(T value)
        {
            _receiver.OnNext(_select(value));
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