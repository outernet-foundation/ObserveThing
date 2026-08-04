using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public static class Settings
    {
        public static Action<Exception> DefaultExceptionHandler = UnityEngine.Debug.LogException;
        public static ObservationContext DefaultObservationContext = new ObservationContext();
    }

    public interface IObserver
    {
        void OnError(Exception exc);
        void OnDispose();
    }

    public interface IOperation
    {
        IOperationObservable source { get; }
        object value { get; }

        IOperation Duplicate();
    }

    public interface IOperationObserver : IObserver
    {
        void OnNext(IOperation operation);
    }

    public class OperationObserver : IOperationObserver
    {
        private Action<IOperation> _onNext;
        private Action _onDispose;
        private Action<Exception> _onError;

        public OperationObserver(Action<IOperation> onNext = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onNext = onNext;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnNext(IOperation args)
        {
            try
            {
                _onNext?.Invoke(args);
            }
            catch (Exception exc)
            {
                OnError(exc);
            }
        }

        public void OnDispose() => _onDispose?.Invoke();
        public void OnError(Exception error) => (_onError ?? Settings.DefaultExceptionHandler)?.Invoke(error);
    }

    public interface IValueObserver<T> : IObserver
    {
        void OnNext(T value);
    }

    public class ValueObserver<T> : IValueObserver<T>
    {
        private Action<T> _onNext;
        private Action _onDispose;
        private Action<Exception> _onError;

        public ValueObserver(Action<T> onNext = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onNext = onNext;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnNext(T value)
            => _onNext?.Invoke(value);

        public void OnDispose()
            => _onDispose?.Invoke();

        public void OnError(Exception exception)
            => _onError?.Invoke(exception);
    }

    public interface ICollectionObserver : IObserver
    {
        void OnAdd(uint elementId, object element);
        void OnRemove(uint elementId, object element);
    }

    public class CollectionObserver : ICollectionObserver
    {
        private Action<uint, object> _onAdd;
        private Action<uint, object> _onRemove;
        private Action _onDispose;
        private Action<Exception> _onError;

        public CollectionObserver(Action<uint, object> onAdd = default, Action<uint, object> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnAdd(uint elementId, object element)
            => _onAdd?.Invoke(elementId, element);

        public void OnRemove(uint elementId, object element)
            => _onRemove?.Invoke(elementId, element);

        public void OnDispose()
            => _onDispose?.Invoke();

        public void OnError(Exception exception)
            => _onError?.Invoke(exception);
    }

    public interface ICollectionObserver<T> : IObserver
    {
        void OnAdd(uint elementId, T element);
        void OnRemove(uint elementId, T element);
    }

    public class CollectionObserver<T> : ICollectionObserver<T>
    {
        private Action<uint, T> _onAdd;
        private Action<uint, T> _onRemove;
        private Action _onDispose;
        private Action<Exception> _onError;

        public CollectionObserver(Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnAdd(uint elementId, T element)
            => _onAdd?.Invoke(elementId, element);

        public void OnRemove(uint elementId, T element)
            => _onRemove?.Invoke(elementId, element);

        public void OnDispose()
            => _onDispose?.Invoke();

        public void OnError(Exception exception)
            => _onError?.Invoke(exception);
    }

    public interface IListObserver : IObserver
    {
        void OnAdd(uint elementId, int index, object element);
        void OnRemove(uint elementId, int index, object element);
    }

    public class ListObserver : IListObserver
    {
        private Action<uint, int, object> _onAdd;
        private Action<uint, int, object> _onRemove;
        private Action _onDispose;
        private Action<Exception> _onError;

        public ListObserver(Action<uint, int, object> onAdd = default, Action<uint, int, object> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnAdd(uint elementId, int index, object element)
            => _onAdd?.Invoke(elementId, index, element);

        public void OnRemove(uint elementId, int index, object element)
            => _onRemove?.Invoke(elementId, index, element);

        public void OnDispose()
            => _onDispose?.Invoke();

        public void OnError(Exception exception)
            => _onError?.Invoke(exception);
    }

    public interface IListObserver<T> : IObserver
    {
        void OnAdd(uint elementId, int index, T element);
        void OnRemove(uint elementId, int index, T element);
    }

    public class ListObserver<T> : IListObserver<T>
    {
        private Action<uint, int, T> _onAdd;
        private Action<uint, int, T> _onRemove;
        private Action _onDispose;
        private Action<Exception> _onError;

        public ListObserver(Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnAdd(uint elementId, int index, T element)
            => _onAdd?.Invoke(elementId, index, element);

        public void OnRemove(uint elementId, int index, T element)
            => _onRemove?.Invoke(elementId, index, element);

        public void OnDispose()
            => _onDispose?.Invoke();

        public void OnError(Exception exception)
            => _onError?.Invoke(exception);
    }

    public interface ISetObserver : IObserver
    {
        void OnAdd(uint elementId, object element);
        void OnRemove(uint elementId, object element);
    }

    public class SetObserver : ISetObserver
    {
        private Action<uint, object> _onAdd;
        private Action<uint, object> _onRemove;
        private Action _onDispose;
        private Action<Exception> _onError;

        public SetObserver(Action<uint, object> onAdd = default, Action<uint, object> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnAdd(uint elementId, object element)
            => _onAdd?.Invoke(elementId, element);

        public void OnRemove(uint elementId, object element)
            => _onRemove?.Invoke(elementId, element);

        public void OnDispose()
            => _onDispose?.Invoke();

        public void OnError(Exception exception)
            => _onError?.Invoke(exception);
    }

    public interface ISetObserver<T> : IObserver
    {
        void OnAdd(uint elementId, T element);
        void OnRemove(uint elementId, T element);
    }

    public class SetObserver<T> : ISetObserver<T>
    {
        private Action<uint, T> _onAdd;
        private Action<uint, T> _onRemove;
        private Action _onDispose;
        private Action<Exception> _onError;

        public SetObserver(Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnAdd(uint elementId, T element)
            => _onAdd?.Invoke(elementId, element);

        public void OnRemove(uint elementId, T element)
            => _onRemove?.Invoke(elementId, element);

        public void OnDispose()
            => _onDispose?.Invoke();

        public void OnError(Exception exception)
            => _onError?.Invoke(exception);
    }

    public interface IDictionaryObserver : IObserver
    {
        void OnAdd(uint elementId, KeyValuePair<object, object> kvp);
        void OnRemove(uint elementId, KeyValuePair<object, object> kvp);
    }

    public class DictionaryObserver : IDictionaryObserver
    {
        private Action<uint, KeyValuePair<object, object>> _onAdd;
        private Action<uint, KeyValuePair<object, object>> _onRemove;
        private Action _onDispose;
        private Action<Exception> _onError;

        public DictionaryObserver(Action<uint, KeyValuePair<object, object>> onAdd = default, Action<uint, KeyValuePair<object, object>> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnAdd(uint elementId, KeyValuePair<object, object> kvp)
            => _onAdd?.Invoke(elementId, kvp);

        public void OnRemove(uint elementId, KeyValuePair<object, object> kvp)
            => _onRemove?.Invoke(elementId, kvp);

        public void OnDispose()
            => _onDispose?.Invoke();

        public void OnError(Exception exception)
            => _onError?.Invoke(exception);
    }

    public interface IDictionaryObserver<TKey, TValue> : IObserver
    {
        void OnAdd(uint elementId, KeyValuePair<TKey, TValue> kvp);
        void OnRemove(uint elementId, KeyValuePair<TKey, TValue> kvp);
    }

    public class DictionaryObserver<TKey, TValue> : IDictionaryObserver<TKey, TValue>
    {
        private Action<uint, KeyValuePair<TKey, TValue>> _onAdd;
        private Action<uint, KeyValuePair<TKey, TValue>> _onRemove;
        private Action _onDispose;
        private Action<Exception> _onError;

        public DictionaryObserver(Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onDispose = onDispose;
            _onError = onError;
        }

        public void OnAdd(uint elementId, KeyValuePair<TKey, TValue> element)
            => _onAdd?.Invoke(elementId, element);

        public void OnRemove(uint elementId, KeyValuePair<TKey, TValue> element)
            => _onRemove?.Invoke(elementId, element);

        public void OnDispose()
            => _onDispose?.Invoke();

        public void OnError(Exception exception)
            => _onError?.Invoke(exception);
    }
}