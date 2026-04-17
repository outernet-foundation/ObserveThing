using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public static class Observers
    {
        public static Action<Exception> DefaultExceptionHandler = UnityEngine.Debug.LogException;
    }

    public interface IObserver
    {
        bool immediate { get; }
        bool prewarm { get; }
        void OnNext(IReadOnlyList<IObservableOperation> operations);
        void OnError(Exception exc);
        void OnDispose();
    }

    public class Observer : IObserver
    {
        public bool immediate { get; }
        public bool prewarm { get; }
        private Action<IReadOnlyList<IObservableOperation>> _onNext;
        private Action<Exception> _onError;
        private Action _onDispose;

        public Observer(Action<IReadOnlyList<IObservableOperation>> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false, bool prewarm = true)
        {
            _onNext = onNext;
            _onError = onError;
            _onDispose = onDispose;
            this.immediate = immediate;
            this.prewarm = prewarm;
        }

        public void OnNext(IReadOnlyList<IObservableOperation> operations)
        {
            try
            {
                _onNext?.Invoke(operations);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnDispose() => _onDispose?.Invoke();
        public void OnError(Exception error) => (_onError ?? Observers.DefaultExceptionHandler)?.Invoke(error);
    }

    public interface IValueObserver<in T>
    {
        bool immediate { get; }
        void OnNext(T value);
        void OnError(Exception exc);
        void OnDispose();
    }

    public class ValueObserver<T> : IValueObserver<T>
    {
        public bool immediate { get; }
        private Action<T> _onNext;
        private Action<Exception> _onError;
        private Action _onDispose;

        public ValueObserver(Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
        {
            _onNext = onNext;
            _onError = onError;
            _onDispose = onDispose;
            this.immediate = immediate;
        }

        public void OnNext(T value)
        {
            try
            {
                _onNext?.Invoke(value);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnDispose() => _onDispose?.Invoke();
        public void OnError(Exception error) => (_onError ?? Observers.DefaultExceptionHandler)?.Invoke(error);
    }

    public interface ICollectionObserver<in T>
    {
        bool immediate { get; }
        public void OnAdd(uint id, T value);
        public void OnRemove(uint id, T value);
        void OnError(Exception exc);
        void OnDispose();
    }

    public class CollectionObserver<T> : ICollectionObserver<T>
    {
        public bool immediate { get; }
        private Action<uint, T> _onAdd;
        private Action<uint, T> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public CollectionObserver(Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
            this.immediate = immediate;
        }

        public void OnAdd(uint id, T value)
        {
            try
            {
                _onAdd?.Invoke(id, value);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnRemove(uint id, T value)
        {
            try
            {
                _onRemove?.Invoke(id, value);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnError(Exception error) => (_onError ?? Observers.DefaultExceptionHandler)?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface ISetObserver<in T>
    {
        bool immediate { get; }
        public void OnAdd(uint id, T value);
        public void OnRemove(uint id, T value);
        void OnError(Exception exc);
        void OnDispose();
    }

    public class SetObserver<T> : ISetObserver<T>
    {
        public bool immediate { get; }
        private Action<uint, T> _onAdd;
        private Action<uint, T> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public SetObserver(Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
            this.immediate = immediate;
        }

        public void OnAdd(uint id, T value)
        {
            try
            {
                _onAdd?.Invoke(id, value);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnRemove(uint id, T value)
        {
            try
            {
                _onRemove?.Invoke(id, value);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnError(Exception error) => (_onError ?? Observers.DefaultExceptionHandler)?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface IListObserver<in T>
    {
        bool immediate { get; }
        public void OnAdd(uint id, int index, T value);
        public void OnRemove(uint id, int index, T value);
        void OnError(Exception exc);
        void OnDispose();
    }

    public class ListObserver<T> : IListObserver<T>
    {
        public bool immediate { get; }
        private Action<uint, int, T> _onAdd;
        private Action<uint, int, T> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public ListObserver(Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
            this.immediate = immediate;
        }

        public void OnAdd(uint id, int index, T value)
        {
            try
            {
                _onAdd?.Invoke(id, index, value);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnRemove(uint id, int index, T value)
        {
            try
            {
                _onRemove?.Invoke(id, index, value);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnError(Exception error) => (_onError ?? Observers.DefaultExceptionHandler)?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface IDictionaryObserver<TKey, TValue>
    {
        bool immediate { get; }
        public void OnAdd(uint id, KeyValuePair<TKey, TValue> keyValuePair);
        public void OnRemove(uint id, KeyValuePair<TKey, TValue> keyValuePair);
        void OnError(Exception exc);
        void OnDispose();
    }

    public class DictionaryObserver<TKey, TValue> : IDictionaryObserver<TKey, TValue>
    {
        public bool immediate { get; }
        private Action<uint, KeyValuePair<TKey, TValue>> _onAdd;
        private Action<uint, KeyValuePair<TKey, TValue>> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public DictionaryObserver(Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
            this.immediate = immediate;
        }

        public void OnAdd(uint id, KeyValuePair<TKey, TValue> keyValuePair)
        {
            try
            {
                _onAdd?.Invoke(id, keyValuePair);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnRemove(uint id, KeyValuePair<TKey, TValue> keyValuePair)
        {
            try
            {
                _onRemove?.Invoke(id, keyValuePair);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnError(Exception error) => (_onError ?? Observers.DefaultExceptionHandler)?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }
}