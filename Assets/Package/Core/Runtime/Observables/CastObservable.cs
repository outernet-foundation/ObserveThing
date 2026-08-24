using System;

namespace ObserveThing
{
    public class CastValueObservable<T> : IDisposable
    {
        private ObservableValue<T> _operand;
        private IDisposable _subscription;

        public CastValueObservable(IValueObservable source, ObservableValue<T> operand)
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

    public class CastCollectionObservable<T> : IDisposable
    {
        private ObservableCollection<T> _operand;
        private IDisposable _subscription;

        public CastCollectionObservable(ICollectionObservable source, ObservableCollection<T> operand)
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

    public class CastListObservable<T> : IDisposable
    {
        private ObservableList<T> _operand;
        private IDisposable _subscription;

        public CastListObservable(IListObservable source, ObservableList<T> operand)
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

    public class CastSetObservable<T> : IDisposable
    {
        private ObservableSet<T> _operand;
        private IDisposable _subscription;

        public CastSetObservable(ISetObservable source, ObservableSet<T> operand)
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

    public class CastDictionaryObservable<TKey, TValue> : IDisposable
    {
        private ObservableDictionary<TKey, TValue> _operand;
        private IDisposable _subscription;

        public CastDictionaryObservable(IDictionaryObservable source, ObservableDictionary<TKey, TValue> operand)
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