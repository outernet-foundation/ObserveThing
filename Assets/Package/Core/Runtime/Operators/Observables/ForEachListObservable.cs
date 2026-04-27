using System;

namespace ObserveThing
{
    public class ForEachListObservable<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private IListObserver<T> _forEachReceiver;
        private IListObserver<T> _receiver;
        private bool _disposed;

        public ForEachListObservable(IListObservable<T> source, IListObserver<T> forEachReceiver, IListObserver<T> receiver)
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

        private void HandleAdd(uint id, int index, T value)
        {
            _forEachReceiver.OnAdd(id, index, value);
            _receiver.OnAdd(id, index, value);
        }

        private void HandleRemove(uint id, int index, T value)
        {
            _forEachReceiver.OnRemove(id, index, value);
            _receiver.OnRemove(id, index, value);
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
