using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class IndexOfObservable<T> : ObservableValueBase<int>
    {
        private IListObservable<T> _source;
        private IValueObservable<T> _valueSource;
        private T _latest = default;
        private List<T> _list = new List<T>();
        private IDisposable _subscriptions;

        public IndexOfObservable(IListObservable<T> source, IValueObservable<T> value) : base(source.context)
        {
            _source = source;
            _valueSource = value;
            SetValueInternal(-1);
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = new ComposedDisposable(

                _valueSource.Subscribe(
                    onNext: HandleNext,
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source.Subscribe(
                    onAdd: HandleAdd,
                    onRemove: HandleRemove,
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            SetValueInternal(-1);
        }

        private void HandleAdd(int index, T element)
        {
            _list.Insert(index, element);
            UpdateIndexIfNecessary();
        }

        private void HandleRemove(int index, T element)
        {
            _list.RemoveAt(index);
            UpdateIndexIfNecessary();
        }

        private void HandleNext(T value)
        {
            _latest = value;
            UpdateIndexIfNecessary();
        }

        private void UpdateIndexIfNecessary()
        {
            var newIndex = -1;

            for (int i = 0; i < _list.Count; i++)
            {
                if (Equals(_latest, _list[i]))
                {
                    newIndex = i;
                    break;
                }
            }

            SetValueInternal(newIndex);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}