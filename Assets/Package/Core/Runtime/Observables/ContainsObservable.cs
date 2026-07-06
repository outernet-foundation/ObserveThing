using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ContainsObservable<T> : ObservableValueBase<bool>
    {
        private ICollectionObservable<T> _source;
        private IValueObservable<T> _valueSource;
        private IDisposable _subscriptions;
        private List<T> _list = new List<T>();
        private T _latest = default;

        public ContainsObservable(ICollectionObservable<T> source, IValueObservable<T> value) : base(source.context, default)
        {
            _source = source;
            _valueSource = value;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = new ComposedDisposable(

                _source.Subscribe(
                    onAdd: HandleAdd,
                    onRemove: HandleRemove,
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _valueSource.Subscribe(
                    onNext: HandleNext,
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
            SetValueInternal(false);
        }

        private void HandleAdd(T element)
        {
            _list.Add(element);

            if (_value)
                return;

            if (Equals(element, _latest))
                SetValueInternal(true);
        }

        private void HandleRemove(T element)
        {
            _list.Remove(element);

            if (!_value)
                return;

            if (Equals(element, _latest) && !_list.Contains(element))
                SetValueInternal(false);
        }

        private void HandleNext(T value)
        {
            _latest = value;
            SetValueInternal(_list.Contains(value));
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
        }
    }
}