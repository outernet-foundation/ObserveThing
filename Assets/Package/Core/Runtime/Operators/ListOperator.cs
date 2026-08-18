using System;

namespace ObserveThing
{
    public class ListOperator<T> : ObservableListBase<T>, IListOperand<T>
    {
        private Func<IListOperand<T>, IDisposable> _generateOperator;
        private IDisposable _operator;
        private bool _active = false;

        public ListOperator(ObservationContext context, Func<IListOperand<T>, IDisposable> generateOperator) : base(context)
        {
            _generateOperator = generateOperator;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _operator = _generateOperator.Invoke(this);
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            _operator?.Dispose();
            _operator = null;

            if (!disposed)
                ClearInternal();
        }

        protected override void DisposeInternal()
        {
            _operator?.Dispose();
            _operator = null;
        }

        void IListOperand<T>.Add(T element)
            => AddInternal(element);

        void IListOperand<T>.Remove(T element)
            => RemoveInternal(element);

        void IListOperand<T>.Insert(int index, T element)
            => InsertInternal(index, element);

        void IListOperand<T>.RemoveAt(int index)
            => RemoveAtInternal(index);

        void IListOperand<T>.Clear()
            => ClearInternal();

        void IOperand.OnError(Exception error)
            => OnError(error);

        void IOperand.OnDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }
    }
}
