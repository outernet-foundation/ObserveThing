using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class TrackObservable<TKey, TValue> : IDisposable
    {
        private IValueOperand<(bool keyPresent, TValue value)> _operand;
        private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        private TKey _key = default;
        private bool _present = false;

        private IDisposable _subscriptions;

        public TrackObservable(IObservable<IDictionaryOperation<TKey, TValue>> source, IObservable<IOperation<TKey>> key, IValueOperand<(bool keyPresent, TValue value)> operand)
        {
            _operand = operand;
            _subscriptions = new ComposedDisposable(

                source.Subscribe(
                    onAdd: kvp =>
                    {
                        _dict.Add(kvp.Key, kvp.Value);
                        if (Equals(kvp.Key, _key))
                        {
                            _present = true;
                            _operand.value = new(_present, kvp.Value);
                        }
                    },
                    onRemove: kvp =>
                    {
                        if (_dict.Remove(kvp.Key) && Equals(kvp.Key, _key))
                        {
                            _present = false;
                            _operand.value = new(_present, default);
                        }
                    },
                    onError: _operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                key.Subscribe(
                    onNext: key =>
                    {
                        _key = key;

                        if (_dict.TryGetValue(_key, out var value))
                        {
                            _present = true;
                            _operand.value = new(_present, value);
                        }
                        else
                        {
                            _present = false;
                            _operand.value = new(_present, default);
                        }
                    },
                    onError: _operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }
}
