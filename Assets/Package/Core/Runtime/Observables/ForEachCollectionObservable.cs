using System;

namespace ObserveThing
{
    public class ForEachCollectionObservable<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private IObserver<ICollectionOperation<T>> _forEachReceiver;
        private IObserver<ICollectionOperation<T>> _receiver;
        private bool _disposed;

        public ForEachCollectionObservable(ICollectionObservable<T> source, IObserver<ICollectionOperation<T>> forEachReceiver, IObserver<ICollectionOperation<T>> receiver)
        {
            _forEachReceiver = forEachReceiver;
            _receiver = receiver;
            _sourceStream = source.Subscribe(
                onNext: operation =>
                {
                    _forEachReceiver.OnNext(operation);
                    _receiver.OnNext(operation);
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