using System;

namespace ObserveThing
{
    public class CastValueObservable<T> : IDisposable
    {
        private IValueOperand<T> _operand;
        private IDisposable _subscription;

        public CastValueObservable(IValueObservable source, IValueOperand<T> operand)
        {
            _operand = operand;
            _subscription = source.Subscribe(
                onNext: x => operand.value = (T)x,
                onError: operand.OnError,
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

    public class CastCollectionObservable<T> : IDisposable
    {
        private ICollectionOperand<T> _operand;
        private IDisposable _subscription;

        public CastCollectionObservable(ICollectionObservable source, ICollectionOperand<T> operand)
        {
            _operand = operand;
            _subscription = source.SubscribeWithId(
                onAdd: (id, x) => operand.Add(id, (T)x),
                onRemove: (id, _) => operand.Remove(id),
                onError: operand.OnError,
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

    public class CastListObservable<T> : IDisposable
    {
        private IListOperand<T> _operand;
        private IDisposable _subscription;

        public CastListObservable(IListObservable source, IListOperand<T> operand)
        {
            _operand = operand;
            _subscription = source.Subscribe(
                onAdd: (index, x) => operand.Insert(index, (T)x),
                onRemove: (index, _) => operand.RemoveAt(index),
                onError: operand.OnError,
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

    public class CastSetObservable<T> : IDisposable
    {
        private ISetOperand<T> _operand;
        private IDisposable _subscription;

        public CastSetObservable(ISetObservable source, ISetOperand<T> operand)
        {
            _operand = operand;
            _subscription = source.Subscribe(
                onAdd: x => operand.Add((T)x),
                onRemove: x => operand.Remove((T)x),
                onError: operand.OnError,
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

    public class CastDictionaryObservable<TKey, TValue> : IDisposable
    {
        private IDictionaryOperand<TKey, TValue> _operand;
        private IDisposable _subscription;

        public CastDictionaryObservable(IDictionaryObservable source, IDictionaryOperand<TKey, TValue> operand)
        {
            _operand = operand;
            _subscription = source.Subscribe(
                onAdd: x => operand.Add((TKey)x.Key, (TValue)x.Value),
                onRemove: x => operand.Remove((TKey)x.Key),
                onError: operand.OnError,
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