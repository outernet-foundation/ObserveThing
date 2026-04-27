using System;

namespace ObserveThing
{
    public class SetOperator<T> : ObservableSetBase<T>, ISetObserver<T>
    {
        public bool immediate => true;
        private Func<ISetObserver<T>, IDisposable> _operatorFactory;
        private bool _active = false;
        private IDisposable _operator;

        public SetOperator(Func<ISetObserver<T>, IDisposable> operatorFactory) : this(default, operatorFactory) { }
        public SetOperator(ObservationContext context, Func<ISetObserver<T>, IDisposable> operatorFactory) : base(context, null)
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
            ClearInternal();
        }

        public void OnDispose()
        {
            _operator = null;

            if (_active)
                Dispose();
        }

        void ISetObserver<T>.OnError(Exception exc)
            => OnError(exc);

        public void OnAdd(uint _, T value)
            => AddInternal(value);

        public void OnRemove(uint _, T value)
            => RemoveInternal(value);
    }
}