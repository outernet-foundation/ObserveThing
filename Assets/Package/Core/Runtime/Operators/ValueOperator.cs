using System;

namespace ObserveThing
{
    public class ValueOperator<T> : ObservableValueBase<T>, IValueObserver<T>
    {
        public bool immediate => true;
        private Func<IValueObserver<T>, IDisposable> _operatorFactory;
        private bool _active = false;
        private IDisposable _operator;

        public ValueOperator(Func<IValueObserver<T>, IDisposable> operatorFactory) : this(default, operatorFactory) { }
        public ValueOperator(ObservationContext context, Func<IValueObserver<T>, IDisposable> operatorFactory) : base(context, default)
        {
            _operatorFactory = operatorFactory;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _operator = _operatorFactory(this);
        }

        protected override void OnLastLastRemoved()
        {
            _active = false;
            _operator?.Dispose();
            _operator = null;
            SetValueInternal(default);
        }

        public void OnDispose()
        {
            _operator = null;

            if (_active)
                Dispose();
        }

        public void OnNext(T value)
        {
            SetValueInternal(value);
        }

        void IValueObserver<T>.OnError(Exception exc)
            => OnError(exc);
    }
}