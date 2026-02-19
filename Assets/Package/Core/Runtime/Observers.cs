using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IObserver
    {
        void OnChange();
        void OnError(Exception error);
        void OnDispose();
    }

    public class Observer : IObserver
    {
        private Action _onChange;
        private Action<Exception> _onError;
        private Action _onDispose;

        public Observer(Action onChange = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onChange = onChange;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnChange() => _onChange?.Invoke();
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface IValueObserver<in T>
    {
        void OnNext(T value);
        void OnError(Exception error);
        void OnDispose();
    }

    public class ValueObserver<T> : IValueObserver<T>
    {
        private Action<T> _onNext;
        private Action<Exception> _onError;
        private Action _onDispose;

        public ValueObserver(Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onNext = onNext;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnNext(T value) => _onNext?.Invoke(value);
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface ICollectionObserver<in T>
    {
        public void OnAdd(uint id, T value);
        public void OnRemove(uint id, T value);
        public void OnError(Exception error);
        public void OnDispose();
    }

    public class CollectionObserver<T> : ICollectionObserver<T>
    {
        private Action<uint, T> _onAdd;
        private Action<uint, T> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public CollectionObserver(Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnAdd(uint id, T value) => _onAdd?.Invoke(id, value);
        public void OnRemove(uint id, T value) => _onRemove?.Invoke(id, value);
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface IListObserver<in T>
    {
        public void OnAdd(uint id, int index, T value);
        public void OnRemove(uint id, int index, T value);
        public void OnError(Exception error);
        public void OnDispose();
    }

    public class ListObserver<T> : IListObserver<T>
    {
        private Action<uint, int, T> _onAdd;
        private Action<uint, int, T> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public ListObserver(Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnAdd(uint id, int index, T value) => _onAdd?.Invoke(id, index, value);
        public void OnRemove(uint id, int index, T value) => _onRemove?.Invoke(id, index, value);
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface IDictionaryObserver<TKey, TValue>
    {
        public void OnAdd(uint id, KeyValuePair<TKey, TValue> keyValuePair);
        public void OnRemove(uint id, KeyValuePair<TKey, TValue> keyValuePair);
        public void OnError(Exception error);
        public void OnDispose();
    }

    public class DictionaryObserver<TKey, TValue> : IDictionaryObserver<TKey, TValue>
    {
        private Action<uint, KeyValuePair<TKey, TValue>> _onAdd;
        private Action<uint, KeyValuePair<TKey, TValue>> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public DictionaryObserver(Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnAdd(uint id, KeyValuePair<TKey, TValue> keyValuePair) => _onAdd?.Invoke(id, keyValuePair);
        public void OnRemove(uint id, KeyValuePair<TKey, TValue> keyValuePair) => _onRemove?.Invoke(id, keyValuePair);
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }
}