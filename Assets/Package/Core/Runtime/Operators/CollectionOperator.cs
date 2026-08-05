using System;

namespace ObserveThing
{
    public class CollectionOperator<T> : ObservableCollectionBase<T>, ICollectionOperand<T>
    {
        private Func<ICollectionOperand<T>, IDisposable> _generateOperator;
        private IDisposable _operator;
        private bool _active = false;

        public CollectionOperator(ObservationContext context, Func<ICollectionOperand<T>, IDisposable> generateOperator) : base(context)
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

        void ICollectionOperand<T>.Add(uint id, T value)
            => AddInternal(id, value);

        void ICollectionOperand<T>.Remove(uint id)
            => RemoveInternal(id);

        void ICollectionOperand<T>.Clear()
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
