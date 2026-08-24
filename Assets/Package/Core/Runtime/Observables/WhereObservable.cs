using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class WhereObservable<T> : IDisposable
    {
        private ObservableCollection<T> _operand;
        private Func<T, IValueObservable<bool>> _where;
        private Dictionary<uint, EntryData> _dataById = new Dictionary<uint, EntryData>();
        private IDisposable _subscriptions;

        private class EntryData
        {
            public bool initialized;
            public T value;
            public bool included;
            public IDisposable subscription;
        }

        public WhereObservable(ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where, ObservableCollection<T> operand)
        {
            _operand = operand;
            _where = where;
            _subscriptions = source.SubscribeWithId(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onDispose: Dispose,
                onError: _operand.OnError,
                immediate: true
            );
        }

        private void HandleAdd(uint id, T value)
        {
            var data = new EntryData() { value = value };
            _dataById.Add(id, data);
            data.subscription = _where(value).Subscribe(
                onNext: included =>
                {
                    data.included = included;

                    if (!data.initialized)
                    {
                        data.initialized = true;

                        if (!included)
                            return;
                    }

                    if (included)
                    {
                        _operand.Add(id, data.value);
                    }
                    else if (data.initialized)
                    {
                        _operand.Remove(id);
                    }
                },
                onError: _operand.OnError,
                immediate: true
            );
        }

        private void HandleRemove(uint id, T value)
        {
            var data = _dataById[id];
            data.subscription.Dispose();
            _dataById.Remove(id);

            if (data.included)
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