using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class SelectCollectionObservable<T, U> : IDisposable
    {
        private IDisposable _sourceStream;
        private Func<T, U> _select;
        private ICollectionObserver<U> _receiver;
        private Dictionary<uint, U> _selected = new Dictionary<uint, U>();
        private bool _disposed;

        public SelectCollectionObservable(ICollectionObservable<T> source, Func<T, U> select, ICollectionObserver<U> receiver)
        {
            _receiver = receiver;
            _select = select;
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
            var selected = _select(value);
            _selected[id] = selected;
            _receiver.OnAdd(id, selected);
        }

        private void HandleRemove(uint id, T value)
        {
            var selected = _selected[id];
            _selected.Remove(id);
            _receiver.OnRemove(id, selected);
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