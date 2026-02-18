using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CollectionShallowCopyDynamic<T> : IDisposable
    {
        private class ElementData
        {
            public IDisposable subscription;
            public int count;
            public T latest;
        }

        private IDisposable _sourceStream;
        private ICollectionObserver<T> _receiver;
        private Dictionary<IValueObservable<T>, ElementData> _dataByElement = new Dictionary<IValueObservable<T>, ElementData>();
        private bool _disposed;

        public CollectionShallowCopyDynamic(ICollectionObservable<IValueObservable<T>> source, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;
            _sourceStream = source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(IValueObservable<T> observable)
        {
            if (!_dataByElement.TryGetValue(observable, out var data))
            {
                data = new ElementData();
                data.subscription = observable.Subscribe(
                    onNext: x =>
                    {
                        for (int i = 0; i < data.count; i++)
                            _receiver.OnRemove(data.latest);

                        data.latest = x;

                        for (int i = 0; i < data.count; i++)
                            _receiver.OnAdd(data.latest);
                    },
                    onError: _receiver.OnError
                );

                _dataByElement.Add(observable, data);
            }

            data.count++;
            _receiver.OnAdd(data.latest);
        }

        private void HandleRemove(IValueObservable<T> observable)
        {
            var data = _dataByElement[observable];
            data.count--;

            if (data.count == 0)
            {
                data.subscription.Dispose();
                _dataByElement.Remove(observable);
            }

            _receiver.OnRemove(data.latest);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            foreach (var data in _dataByElement.Values)
                data.subscription.Dispose();

            _receiver.OnDispose();
        }
    }
}