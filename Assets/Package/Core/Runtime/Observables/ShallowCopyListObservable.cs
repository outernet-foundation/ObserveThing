using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ShallowCopyListObservable<T> : ObservableListBase<T>
    {
        private IListObservable<IValueObservable<T>> _source;
        private List<EntryData> _data = new List<EntryData>();
        private IDisposable _subscriptions;
        private bool _active;

        private class EntryData
        {
            public IDisposable subscription;
            public bool initialized;
        }

        public ShallowCopyListObservable(IListObservable<IValueObservable<T>> source) : base(source.context)
        {
            _source = source;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = _source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: OnError,
                onDispose: HandleSourceDisposed,
                immediate: true
            );
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            _subscriptions?.Dispose();
            _subscriptions = null;

            foreach (var data in _data)
                data.subscription.Dispose();

            _data.Clear();
            ClearInternal();
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
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
                        RemoveAtInternal(index);

                    data.initialized = true;
                    InsertInternal(index, x);
                },
                onError: OnError,
                immediate: true
            );
        }

        private void HandleRemove(int index, IValueObservable<T> element)
        {
            var data = _data[index];
            _data.RemoveAt(index);
            data.subscription.Dispose();
            RemoveAtInternal(index);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            foreach (var data in _data)
                data.subscription.Dispose();
        }
    }
}
