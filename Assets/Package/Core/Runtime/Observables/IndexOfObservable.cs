using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class IndexOfObservable<T> : IDisposable
    {
        private IValueOperand<(bool found, int index)> _operand;
        private T _latest = default;
        private List<T> _list = new List<T>();
        private IDisposable _subscriptions;

        public IndexOfObservable(IObservable<IListOperation<T>> source, IObservable<IOperation<T>> value, IValueOperand<(bool found, int index)> operand)
        {
            _operand = operand;
            _subscriptions = new ComposedDisposable(

                value.Subscribe(
                    onNext: HandleNext,
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source.Subscribe(
                    onAdd: HandleAdd,
                    onRemove: HandleRemove,
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
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

            if (newIndex == -1)
            {
                _operand.value = default;
                return;
            }

            _operand.value = new(true, newIndex);
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }
}