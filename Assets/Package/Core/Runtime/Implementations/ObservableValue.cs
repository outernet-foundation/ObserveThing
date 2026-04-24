using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableValue<T> : ObservableValueBase<T>
    {
        public T value
        {
            get => _value;
            set => SetValueInternal(value);
        }

        public ObservableValue(ObservationContext context, T value) : base(context, value) { }
        public ObservableValue(ObservationContext context) : this(context, default) { }
        public ObservableValue(T value) : this(default, value) { }
        public ObservableValue() : this(default, default) { }
    }
}