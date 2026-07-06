using System;

namespace ObserveThing
{
    public class SelectValueObservable<T, U> : IDisposable
    {
        private IDisposable _sourceStream;
        private Func<T, U> _select;
        private IObserver<IValueOperation<U>> _receiver;
        private U _selected;
        private bool _disposed;

        public SelectValueObservable(IValueObservable<T> source, Func<T, U> select, IObserver<IValueOperation<U>> receiver)
        {
            _receiver = receiver;
            _select = select;
            _sourceStream = source.Subscribe(
                onNext: HandleNext,
                onError: _receiver.OnError,
                onDispose: Dispose,
                immediate: receiver.immediate
            );

            // Always send init call
            if (Equals(_selected, default(U)))
                _receiver.OnNext(default);
        }

        private void HandleNext(T value)
        {
            var nextSelect = _select(value);

            if (Equals(nextSelect, _selected))
                return;

            _selected = nextSelect;
            _receiver.OnNext(_selected);
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