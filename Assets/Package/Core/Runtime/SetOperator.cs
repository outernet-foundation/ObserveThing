using System;

namespace ObserveThing
{
    public class SetOperator<T> : ObservableSetBase<T>, ISetOperand<T>
    {
        private Func<ISetOperand<T>, IDisposable> _generateOperator;
        private IDisposable _operator;
        private bool _active = false;

        public SetOperator(ObservationContext context, Func<ISetOperand<T>, IDisposable> generateOperator) : base(context)
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

        void ISetOperand<T>.Add(T value)
            => AddInternal(value);

        void ISetOperand<T>.Remove(T value)
            => RemoveInternal(value);

        void ISetOperand<T>.Clear()
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
