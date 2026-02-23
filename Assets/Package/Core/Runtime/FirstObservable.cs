using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class FirstObservable<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private List<(uint id, T value)> _filteredList = new List<(uint id, T value)>();
        private IValueObserver<(bool found, T value)> _receiver;
        private (uint id, T value) _latest;
        private bool _disposed;
        private bool _latestIsDefault => _latest.id == default && Equals(_latest.value, default);

        public FirstObservable(ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate, IValueObserver<(bool found, T value)> receiver)
        {
            _receiver = receiver;
            _sourceStream = source.ObservableWhere(x => validate(x)).SubscribeWithId(
                onAdd: (id, value) =>
                {
                    _filteredList.Add(new(id, value));
                    _filteredList.Sort((x, y) => x.id.CompareTo(y.id));
                    NotifyReceiverIfNecessary();
                },
                onRemove: (id, value) =>
                {
                    _filteredList.Remove(new(id, value));
                    NotifyReceiverIfNecessary();
                },
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void NotifyReceiverIfNecessary()
        {
            var next = _filteredList.Count == 0 ? new(0, default) : _filteredList[0];

            if (_latest.id == next.id && Equals(_latest.value, next.value))
                return;

            _latest = next;
            _receiver.OnNext(new(_filteredList.Count != 0, _latest.value));
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