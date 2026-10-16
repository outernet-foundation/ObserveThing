using System;

namespace ObserveThing
{
    public class CastValueOperator<T> : IDisposable
    {
        private ObservableValue<T> _operand;
        private IDisposable _subscription;

        public CastValueOperator(IValueObservable source, ObservableValue<T> operand)
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
            _operand.Dispose();
        }
    }

    public class CastCollectionOperator<T> : IDisposable
    {
        private ObservableCollection<T> _operand;
        private IDisposable _subscription;

        public CastCollectionOperator(ICollectionObservable source, ObservableCollection<T> operand)
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
            _operand.Dispose();
        }
    }

    public class CastListOperator<T> : IDisposable
    {
        private ObservableList<T> _operand;
        private IDisposable _subscription;

        public CastListOperator(IListObservable source, ObservableList<T> operand)
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
            _operand.Dispose();
        }
    }

    public class CastSetOperator<T> : IDisposable
    {
        private ObservableSet<T> _operand;
        private IDisposable _subscription;

        public CastSetOperator(ISetObservable source, ObservableSet<T> operand)
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
            _operand.Dispose();
        }
    }

    public class CastDictionaryOperator<TKey, TValue> : IDisposable
    {
        private ObservableDictionary<TKey, TValue> _operand;
        private IDisposable _subscription;

        public CastDictionaryOperator(IDictionaryObservable source, ObservableDictionary<TKey, TValue> operand)
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
            _operand.Dispose();
        }
    }
}