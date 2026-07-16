using System;

namespace ObserveThing
{
    public class CastCollectionObservable<T> : IDisposable
    {
        private ICollectionOperand<T> _operand;
        private IDisposable _subscription;

        public CastCollectionObservable(ICollectionObservable source, ICollectionOperand<T> operand)
        {
            _operand = operand;
            _subscription = source.SubscribeWithId(
                onAdd: (id, x) => _operand.Add(id, (T)x),
                onRemove: (id, x) => _operand.Remove(id),
                onError: _operand.OnError,
                onDispose: Dispose,
                immediate: true
            );
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
            _operand.OnDisposed();
        }
    }
}