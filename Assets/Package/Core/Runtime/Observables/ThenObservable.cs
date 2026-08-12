using System;

namespace ObserveThing
{
    public class ThenValueObservable<T> : IDisposable
    {
        private IValueObservable<T> _source;
        private IValueObserver<T> _then;
        private IValueOperand<T> _operand;
        private IDisposable _subscriptions;

        public ThenValueObservable(IValueObservable<T> source, IValueObserver<T> then, IValueOperand<T> operand)
        {
            _source = source;
            _then = then;
            _operand = operand;
            _subscriptions = _source.Subscribe(
                onNext: x =>
                {
                    _then.OnNext(x);
                    _operand.value = x;
                },
                onError: exc =>
                {
                    _then.OnError(exc);
                    _operand.OnError(exc);
                },
                onDispose: Dispose
            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            _then.OnDispose();
            _operand.OnDisposed();
        }
    }

    public class ThenCollectionObservable<T> : IDisposable
    {
        private ICollectionObservable<T> _source;
        private ICollectionObserver<T> _then;
        private ICollectionOperand<T> _operand;
        private IDisposable _subscriptions;

        public ThenCollectionObservable(ICollectionObservable<T> source, ICollectionObserver<T> then, ICollectionOperand<T> operand)
        {
            _source = source;
            _then = then;
            _operand = operand;
            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) =>
                {
                    then.OnAdd(id, element);
                    operand.Add(id, element);
                },
                onRemove: (id, element) =>
                {
                    then.OnRemove(id, element);
                    operand.Remove(id);
                },
                onError: exc =>
                {
                    _then.OnError(exc);
                    _operand.OnError(exc);
                },
                onDispose: Dispose
            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            _then.OnDispose();
            _operand.OnDisposed();
        }
    }

    public class ThenListObservable<T> : IDisposable
    {
        private IListObservable<T> _source;
        private IListObserver<T> _then;
        private IListOperand<T> _operand;
        private IDisposable _subscriptions;

        public ThenListObservable(IListObservable<T> source, IListObserver<T> then, IListOperand<T> operand)
        {
            _source = source;
            _then = then;
            _operand = operand;
            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, index, element) =>
                {
                    then.OnAdd(id, index, element);
                    operand.Insert(index, element);
                },
                onRemove: (id, index, element) =>
                {
                    then.OnRemove(id, index, element);
                    operand.RemoveAt(index);
                },
                onError: exc =>
                {
                    _then.OnError(exc);
                    _operand.OnError(exc);
                },
                onDispose: Dispose
            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            _then.OnDispose();
            _operand.OnDisposed();
        }
    }

    public class ThenSetObservable<T> : IDisposable
    {
        private ISetObservable<T> _source;
        private ISetObserver<T> _then;
        private ISetOperand<T> _operand;
        private IDisposable _subscriptions;

        public ThenSetObservable(ISetObservable<T> source, ISetObserver<T> then, ISetOperand<T> operand)
        {
            _source = source;
            _then = then;
            _operand = operand;
            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) =>
                {
                    then.OnAdd(id, element);
                    operand.Add(element);
                },
                onRemove: (id, element) =>
                {
                    then.OnRemove(id, element);
                    operand.Remove(element);
                },
                onError: exc =>
                {
                    _then.OnError(exc);
                    _operand.OnError(exc);
                },
                onDispose: Dispose
            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            _then.OnDispose();
            _operand.OnDisposed();
        }
    }

    public class ThenDictionaryObservable<TKey, TValue> : IDisposable
    {
        private IDictionaryObservable<TKey, TValue> _source;
        private IDictionaryObserver<TKey, TValue> _then;
        private IDictionaryOperand<TKey, TValue> _operand;
        private IDisposable _subscriptions;

        public ThenDictionaryObservable(IDictionaryObservable<TKey, TValue> source, IDictionaryObserver<TKey, TValue> then, IDictionaryOperand<TKey, TValue> operand)
        {
            _source = source;
            _then = then;
            _operand = operand;
            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, kvp) =>
                {
                    then.OnAdd(id, kvp);
                    operand.Add(kvp.Key, kvp.Value);
                },
                onRemove: (id, kvp) =>
                {
                    then.OnRemove(id, kvp);
                    operand.Remove(kvp.Key);
                },
                onError: exc =>
                {
                    _then.OnError(exc);
                    _operand.OnError(exc);
                },
                onDispose: Dispose
            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            _then.OnDispose();
            _operand.OnDisposed();
        }
    }
}