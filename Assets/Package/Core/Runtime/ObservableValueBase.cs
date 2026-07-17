using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableValueBase<T> : Observable<T>
    {
        protected T _value { get; private set; }
        private T[] _initOperations = new T[1];

        public ObservableValueBase(ObservationContext context) : this(context, default) { }
        public ObservableValueBase(ObservationContext context, T value) : base(context)
        {
            _value = value;
        }

        public override IReadOnlyList<T> GetInitializationOperations()
        {
            _initOperations[0] = _value;
            return _initOperations;
        }

        protected void SetValueInternal(T value)
        {
            if (Equals(_value, value))
                return;

            _value = value;
            EnqueuePendingOperation(value);
        }
    }
}