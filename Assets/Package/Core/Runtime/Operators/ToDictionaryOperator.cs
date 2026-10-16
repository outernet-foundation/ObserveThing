using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ToDictionaryOperator<T, TKey, TValue> : IDisposable
    {
        private IDisposable _subscription;
        private ObservableDictionary<TKey, TValue> _operand;

        public ToDictionaryOperator(ICollectionObservable<T> source, Func<T, IValueObservable<TKey>> selectKey, Func<T, IValueObservable<TValue>> selectValue, ObservableDictionary<TKey, TValue> operand)
        {
            _operand = operand;
            _subscription = source.ObservableSelect(
                x => Operators.ObservableCombineValues(
                    selectKey(x),
                    selectValue(x),
                    (key, value) => new KeyValuePair<TKey, TValue>(key, value)
                )
            ).Subscribe(
                onAdd: kvp => _operand.Add(kvp.Key, kvp.Value),
                onRemove: kvp => _operand.Remove(kvp.Key),
                onError: operand.OnError,
                onDispose: Dispose
            );
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
            _operand.Dispose();
        }
    }
}
