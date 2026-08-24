using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class UnwrapCollectionObservable<T> : IDisposable
    {
        private ObservableCollection<T> _operand;
        private Dictionary<uint, EntryData> _dataById = new Dictionary<uint, EntryData>();
        private IDisposable _subscriptions;

        private class EntryData
        {
            public IDisposable subscription;
            public bool initialized;
        }

        public UnwrapCollectionObservable(ICollectionObservable<IValueObservable<T>> source, ObservableCollection<T> operand)
        {
            _operand = operand;
            _subscriptions = source.Subscribe(new CollectionObserver<IValueObservable<T>>(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: operand.OnError,
                onDispose: Dispose
            ), immediate: true);
        }

        private void HandleAdd(uint id, IValueObservable<T> observable)
        {
            var data = new EntryData();
            _dataById.Add(id, data);
            data.subscription = observable.Subscribe(new ValueObserver<T>(
                onNext: x =>
                {
                    if (data.initialized)
                        _operand.Remove(id);

                    data.initialized = true;
                    _operand.Add(id, x);
                },
                onError: _operand.OnError
            ), immediate: true);
        }

        private void HandleRemove(uint id, IValueObservable<T> observable)
        {
            var data = _dataById[id];
            _dataById.Remove(id);
            data.subscription.Dispose();
            _operand.Remove(id);
        }

        public void Dispose()
        {
            foreach (var data in _dataById.Values)
                data.subscription.Dispose();

            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }
}