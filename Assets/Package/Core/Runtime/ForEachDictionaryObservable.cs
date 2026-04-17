using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ForEachDictionaryObservable<TKey, TValue> : IDisposable
    {
        private IDisposable _sourceStream;
        private IDictionaryObserver<TKey, TValue> _forEachReceiver;
        private IDictionaryObserver<TKey, TValue> _receiver;
        private bool _disposed;

        public ForEachDictionaryObservable(IDictionaryOperator<TKey, TValue> source, IDictionaryObserver<TKey, TValue> forEachReceiver, IDictionaryObserver<TKey, TValue> receiver)
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

        private void HandleAdd(uint id, KeyValuePair<TKey, TValue> value)
        {
            _forEachReceiver.OnAdd(id, value);
            _receiver.OnAdd(id, value);
        }

        private void HandleRemove(uint id, KeyValuePair<TKey, TValue> value)
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