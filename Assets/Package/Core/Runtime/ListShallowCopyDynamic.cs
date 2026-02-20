using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ListShallowCopyDynamic<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private IListObserver<T> _receiver;
        private List<EntryData> _data = new List<EntryData>();
        private bool _disposed;

        private class EntryData
        {
            public T latest;
            public IDisposable subscription;
            public bool initialized;
        }

        public ListShallowCopyDynamic(IListObservable<IValueObservable<T>> source, IListObserver<T> receiver)
        {
            _receiver = receiver;
            _sourceStream = source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(uint id, int index, IValueObservable<T> element)
        {
            var data = new EntryData();
            _data.Insert(index, data);
            data.subscription = element.Subscribe(
                onNext: x =>
                {
                    var index = _data.IndexOf(data);

                    if (!data.initialized)
                    {
                        data.latest = x;
                        _receiver.OnAdd(id, index, data.latest);
                        data.initialized = true;
                        return;
                    }

                    _receiver.OnRemove(id, index, data.latest);
                    data.latest = x;
                    _receiver.OnAdd(id, index, data.latest);
                },
                onError: _receiver.OnError
            );
        }

        private void HandleRemove(uint id, int index, IValueObservable<T> element)
        {
            var data = _data[index];
            _data.RemoveAt(index);
            data.subscription.Dispose();
            _receiver.OnRemove(id, index, data.latest);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            foreach (var data in _data)
                data.subscription.Dispose();

            _receiver.OnDispose();
        }
    }
}
