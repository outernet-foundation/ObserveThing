using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class FirstObservable<T> : ObservableValueBase<T>
    {
        private ICollectionObservable<T> _source;
        private Func<T, IValueObservable<bool>> _validate;
        private List<(uint id, T value)> _sortedList = new List<(uint id, T value)>();
        private (uint id, T value) _latest;
        private IDisposable _subscriptions;

        public FirstObservable(ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate) : base(source.context)
        {
            _source = source;
            _validate = validate;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = _source.ObservableWhere(x => _validate(x)).SubscribeWithId(
                onAdd: (id, value) =>
                {
                    _sortedList.Add(new(id, value));
                    _sortedList.Sort((x, y) => x.id.CompareTo(y.id));
                    SetValueIfNecessary();
                },
                onRemove: (id, value) =>
                {
                    _sortedList.Remove(new(id, value));
                    SetValueIfNecessary();
                },
                onError: OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            SetValueInternal(default);
        }

        private void SetValueIfNecessary()
        {
            var next = _sortedList.Count == 0 ? new(0, default) : _sortedList[0];

            if (_latest.id == next.id && Equals(_latest.value, next.value))
                return;

            _latest = next;
            SetValueInternal(_latest.value);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}