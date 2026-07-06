using System;

namespace ObserveThing
{
    public class ForEachListObservable<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private IObserver<IListOperation<T>> _forEachReceiver;
        private IObserver<IListOperation<T>> _receiver;
        private bool _disposed;

        public ForEachListObservable(IListObservable<T> source, IObserver<IListOperation<T>> forEachReceiver, IObserver<IListOperation<T>> receiver)
        {
            _forEachReceiver = forEachReceiver;
            _receiver = receiver;
            _sourceStream = source.Subscribe(
                onNext: op =>
                {
                    _forEachReceiver.OnNext(op);
                    _receiver.OnNext(op);
                },
                onError: _receiver.OnError,
                onDispose: Dispose,
                immediate: receiver.immediate
            );
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _forEachReceiver.OnDispose();
            _receiver.OnDispose();
        }
    }
}
