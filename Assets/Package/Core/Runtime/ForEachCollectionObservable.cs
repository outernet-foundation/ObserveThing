using System;

namespace ObserveThing
{
    public class ForEachCollectionObservable<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private ICollectionObserver<T> _forEachReceiver;
        private ICollectionObserver<T> _receiver;
        private bool _disposed;

        public ForEachCollectionObservable(ICollectionOperator<T> source, ICollectionObserver<T> forEachReceiver, ICollectionObserver<T> receiver)
        {
            _forEachReceiver = forEachReceiver;
            _receiver = receiver;
            _sourceStream = source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose,
                immediate: receiver.immediate
            );
        }

        private void HandleAdd(uint id, T value)
        {
            _forEachReceiver.OnAdd(id, value);
            _receiver.OnAdd(id, value);
        }

        private void HandleRemove(uint id, T value)
        {
            _forEachReceiver.OnRemove(id, value);
            _receiver.OnRemove(id, value);
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