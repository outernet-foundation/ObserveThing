using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class UnwrapListObservable<T> : IDisposable
    {
        private IListOperand<T> _operand;
        private List<EntryData> _data = new List<EntryData>();
        private IDisposable _subscriptions;

        private class EntryData
        {
            public IDisposable subscription;
            public bool initialized;
        }

        public UnwrapListObservable(IListObservable<IValueObservable<T>> source, IListOperand<T> operand)
        {
            _operand = operand;
            _subscriptions = source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        private void HandleAdd(int index, IValueObservable<T> element)
        {
            var data = new EntryData();
            _data.Insert(index, data);
            data.subscription = element.Subscribe(
                onNext: x =>
                {
                    var index = _data.IndexOf(data);

                    if (data.initialized)
                        _operand.RemoveAt(index);

                    data.initialized = true;
                    _operand.Insert(index, x);
                },
                onError: _operand.OnError,
                immediate: true
            );
        }

        private void HandleRemove(int index, IValueObservable<T> element)
        {
            var data = _data[index];
            _data.RemoveAt(index);
            data.subscription.Dispose();
            _operand.RemoveAt(index);
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            foreach (var data in _data)
                data.subscription.Dispose();

            _operand.OnDisposed();
        }
    }
}
