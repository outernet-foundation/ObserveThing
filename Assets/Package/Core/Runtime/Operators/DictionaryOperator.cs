using System;

namespace ObserveThing
{
    public class DictionaryOperator<TKey, TValue> : ObservableDictionaryBase<TKey, TValue>, IDictionaryOperand<TKey, TValue>
    {
        private Func<IDictionaryOperand<TKey, TValue>, IDisposable> _generateOperator;
        private IDisposable _operator;
        private bool _active = false;

        public DictionaryOperator(ObservationContext context, Func<IDictionaryOperand<TKey, TValue>, IDisposable> generateOperator) : base(context)
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

        void IDictionaryOperand<TKey, TValue>.Add(TKey key, TValue value)
            => AddInternal(key, value);

        void IDictionaryOperand<TKey, TValue>.Remove(TKey key)
            => RemoveInternal(key);

        void IDictionaryOperand<TKey, TValue>.Clear()
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
