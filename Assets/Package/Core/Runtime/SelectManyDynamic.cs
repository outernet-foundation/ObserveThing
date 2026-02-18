using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class SelectManyDynamic<T, U> : IDisposable
    {
        private class ElementData
        {
            public List<U> selected = new List<U>();
            public IDisposable subscription;
            public int count;
        }

        private IDisposable _sourceStream;
        private Func<T, ICollectionObservable<U>> _select;
        private ICollectionObserver<U> _receiver;
        private Dictionary<T, ElementData> _dataByElement = new Dictionary<T, ElementData>();
        private bool _disposed;

        public SelectManyDynamic(ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> select, ICollectionObserver<U> receiver)
        {
            _receiver = receiver;
            _select = select;
            source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(T element)
        {
            if (!_dataByElement.TryGetValue(element, out var data))
            {
                data = new ElementData();
                data.subscription = _select(element).Subscribe(
                    onAdd: x =>
                    {
                        data.selected.Add(x);

                        for (int i = 0; i < data.count; i++)
                            _receiver.OnAdd(x);
                    },
                    onRemove: x =>
                    {
                        data.selected.Remove(x);

                        for (int i = 0; i < data.count; i++)
                            _receiver.OnRemove(x);
                    },
                    onError: _receiver.OnError
                );

                _dataByElement.Add(element, data);
            }

            data.count++;

            foreach (var selected in data.selected)
                _receiver.OnAdd(selected);
        }

        private void HandleRemove(T element)
        {
            var data = _dataByElement[element];
            data.count--;

            if (data.count == 0)
            {
                _dataByElement.Remove(element);
                data.subscription.Dispose();
            }

            foreach (var selected in data.selected)
                _receiver.OnRemove(selected);
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