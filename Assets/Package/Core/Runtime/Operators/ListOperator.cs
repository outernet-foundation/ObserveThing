using System;

namespace ObserveThing
{
    public class ListOperator<T> : ObservableListBase<T>, IListObserver<T>
    {
        public bool immediate => true;
        private Func<IListObserver<T>, IDisposable> _operatorFactory;
        private bool _active = false;
        private IDisposable _operator;

        public ListOperator(Func<IListObserver<T>, IDisposable> operatorFactory) : this(default, operatorFactory) { }
        public ListOperator(ObservationContext context, Func<IListObserver<T>, IDisposable> operatorFactory) : base(context, null)
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

        void IListObserver<T>.OnError(Exception exc)
            => OnError(exc);

        public void OnAdd(uint _, int index, T value)
            => InsertInternal(index, value);

        public void OnRemove(uint _, int index, T value)
            => RemoveAtInternal(index);
    }
}