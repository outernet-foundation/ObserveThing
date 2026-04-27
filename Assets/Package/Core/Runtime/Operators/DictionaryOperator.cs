using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class DictionaryOperator<TKey, TValue> : ObservableDictionaryBase<TKey, TValue>, IDictionaryObserver<TKey, TValue>
    {
        public bool immediate => true;
        private Func<IDictionaryObserver<TKey, TValue>, IDisposable> _operatorFactory;
        private bool _active = false;
        private IDisposable _operator;

        public DictionaryOperator(Func<IDictionaryObserver<TKey, TValue>, IDisposable> operatorFactory) : this(default, operatorFactory) { }
        public DictionaryOperator(ObservationContext context, Func<IDictionaryObserver<TKey, TValue>, IDisposable> operatorFactory) : base(context, null)
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

        void IDictionaryObserver<TKey, TValue>.OnError(Exception exc)
            => OnError(exc);

        public void OnAdd(uint _, KeyValuePair<TKey, TValue> value)
            => AddInternal(value.Key, value.Value);

        public void OnRemove(uint _, KeyValuePair<TKey, TValue> value)
            => RemoveInternal(value.Key);
    }
}