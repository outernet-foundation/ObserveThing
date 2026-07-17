using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ContainsObservable<T> : IDisposable
    {
        private IValueOperand<bool> _operand;
        private List<T> _list = new List<T>();
        private T _latest = default;
        private IDisposable _subscriptions;

        public ContainsObservable(IObservable<CollectionOp<T>> source, IObservable<T> value, IValueOperand<bool> operand)
        {
            _operand = operand;
            _subscriptions = new ComposedDisposable(

                source.Subscribe(
                    onAdd: HandleAdd,
                    onRemove: HandleRemove,
                    onError: _operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                value.Subscribe(
                    onNext: HandleNext,
                    onError: _operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        private void HandleAdd(T element)
        {
            _list.Add(element);

            if (_operand.value)
                return;

            if (Equals(element, _latest))
                _operand.value = true;
        }

        private void HandleRemove(T element)
        {
            _list.Remove(element);

            if (!_operand.value)
                return;

            if (Equals(element, _latest) && !_list.Contains(element))
                _operand.value = false;
        }

        private void HandleNext(T value)
        {
            _latest = value;
            _operand.value = _list.Contains(value);
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }
}