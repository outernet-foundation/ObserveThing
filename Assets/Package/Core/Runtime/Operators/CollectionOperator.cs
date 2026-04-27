using System;

namespace ObserveThing
{
    public class CollectionOperator<T> : ObservableCollectionBase<T>, ICollectionObserver<T>
    {
        public bool immediate => true;
        private Func<ICollectionObserver<T>, IDisposable> _operatorFactory;
        private bool _active = false;
        private IDisposable _operator;

        public CollectionOperator(Func<ICollectionObserver<T>, IDisposable> operatorFactory) : this(default, operatorFactory) { }
        public CollectionOperator(ObservationContext context, Func<ICollectionObserver<T>, IDisposable> operatorFactory) : base(context)
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

        void ICollectionObserver<T>.OnError(Exception exc)
            => OnError(exc);

        public void OnAdd(uint id, T value)
            => AddInternal(id, value);

        public void OnRemove(uint id, T _)
            => RemoveInternal(id);
    }
}