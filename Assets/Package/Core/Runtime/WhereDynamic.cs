using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class WhereDynamic<T> : IDisposable
    {
        private class ElementData
        {
            public T element;
            public int count;
            public bool included;
            public IDisposable subscription;
        }

        private IDisposable _sourceStream;
        private ICollectionObserver<T> _receiver;
        private Func<T, IValueObservable<bool>> _where;
        private Dictionary<T, ElementData> _dataByElement = new Dictionary<T, ElementData>();
        private bool _disposed;

        public WhereDynamic(ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;
            _where = where;
            _sourceStream = source.Subscribe(
                HandleAdd,
                HandleRemove,
                _receiver.OnError,
                Dispose
            );
        }

        private void HandleAdd(T value)
        {
            if (!_dataByElement.TryGetValue(value, out var data))
            {
                data = new ElementData() { element = value };
                data.subscription = _where(value).Subscribe(
                    onNext: x =>
                    {
                        if (data.included == x)
                            return;

                        data.included = x;

                        if (data.included)
                        {
                            for (int i = 0; i < data.count; i++)
                                _receiver.OnAdd(data.element);
                        }
                        else
                        {
                            for (int i = 0; i < data.count; i++)
                                _receiver.OnRemove(data.element);
                        }
                    },
                    onError: _receiver.OnError
                );

                _dataByElement.Add(value, data);
            }

            data.count++;

            if (data.included)
                _receiver.OnAdd(value);
        }

        private void HandleRemove(T value)
        {
            var data = _dataByElement[value];
            data.count--;

            if (data.count == 1)
            {
                _dataByElement.Remove(value);
                data.subscription.Dispose();
            }

            if (data.included)
                _receiver.OnRemove(value);
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