using System;

namespace ObserveThing
{
    public class ValueOperator<T> : ObservableValueBase<T>, IValueOperand<T>
    {
        T IValueOperand<T>.value
        {
            get => _value;
            set => SetValueInternal(value);
        }

        private Func<IValueOperand<T>, IDisposable> _generateOperator;
        private IDisposable _operator;
        private bool _active = false;

        public ValueOperator(ObservationContext context, Func<IValueOperand<T>, IDisposable> generateOperator) : base(context)
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
                SetValueInternal(default);
        }

        protected override void DisposeInternal()
        {
            _operator?.Dispose();
            _operator = null;
        }

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