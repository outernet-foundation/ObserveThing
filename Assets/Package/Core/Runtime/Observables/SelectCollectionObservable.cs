using System;

namespace ObserveThing
{
    public class SelectCollectionObservable<T, U> : IDisposable
    {
        private ICollectionOperand<U> _operand;
        private IDisposable _subscriptions;

        public SelectCollectionObservable(IObservable<ICollectionOperation<T>> source, Func<T, U> select, ICollectionOperand<U> operand)
        {
            _operand = operand;
            _subscriptions = source.SubscribeWithId(
                onAdd: (id, value) => operand.Add(id, select(value)),
                onRemove: (id, _) => operand.Remove(id),
                onError: operand.OnError,
                onDispose: Dispose,
                immediate: true
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