using System;
using System.Collections.Generic;
using Unity.Android.Gradle;

namespace ObserveThing
{
    public class ValueOperationStreamObservable<T> : IInitializationOperationsProvider<ValueOp<T>>, IDisposable
    {
        private IValueObservable<T> _source;
        private IObservableOperand<ValueOp<T>> _operand;
        private IDisposable _subscriptions;

        public ValueOperationStreamObservable(IValueObservable<T> source, IObservableOperand<ValueOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.Subscribe(
                onNext: x => operand.EnqueuePendingOperation(new() { source = _source, value = x }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IReadOnlyList<ValueOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<ValueOp<T>>();
            var subscription = _source.Subscribe(x => initOperations.Add(new ValueOp<T>() { source = _source, value = x }));
            subscription.Dispose();
            return initOperations;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }

    public class CollectionOperationStreamObservable<T> : IInitializationOperationsProvider<CollectionOp<T>>, IDisposable
    {
        private ICollectionObservable<T> _source;
        private IObservableOperand<CollectionOp<T>> _operand;
        private IDisposable _subscriptions;

        public CollectionOperationStreamObservable(ICollectionObservable<T> source, IObservableOperand<CollectionOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) => operand.EnqueuePendingOperation(new() { source = _source, elementId = id, element = element, isRemove = false }),
                onRemove: (id, element) => operand.EnqueuePendingOperation(new() { source = _source, elementId = id, element = element, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IReadOnlyList<CollectionOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<CollectionOp<T>>();
            var subscription = _source.SubscribeWithId((id, element) => initOperations.Add(new CollectionOp<T>() { source = _source, elementId = id, element = element }));
            subscription.Dispose();
            return initOperations;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }

    public class ListOperationStreamObservable<T> : IInitializationOperationsProvider<ListOp<T>>, IDisposable
    {
        private IListObservable<T> _source;
        private IObservableOperand<ListOp<T>> _operand;
        private IDisposable _subscriptions;

        public ListOperationStreamObservable(IListObservable<T> source, IObservableOperand<ListOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, index, element) => operand.EnqueuePendingOperation(new() { source = _source, elementId = id, index = index, element = element, isRemove = false }),
                onRemove: (id, index, element) => operand.EnqueuePendingOperation(new() { source = _source, elementId = id, index = index, element = element, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IReadOnlyList<ListOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<ListOp<T>>();
            var subscription = _source.SubscribeWithId((id, index, element) => initOperations.Add(new ListOp<T>() { source = _source, elementId = id, index = index, element = element }));
            subscription.Dispose();
            return initOperations;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }

    public class DictionaryOperationStreamObservable<TKey, TValue> : IInitializationOperationsProvider<DictionaryOp<TKey, TValue>>, IDisposable
    {
        private IDictionaryObservable<TKey, TValue> _source;
        private IObservableOperand<DictionaryOp<TKey, TValue>> _operand;
        private IDisposable _subscriptions;

        public DictionaryOperationStreamObservable(IDictionaryObservable<TKey, TValue> source, IObservableOperand<DictionaryOp<TKey, TValue>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, kvp) => operand.EnqueuePendingOperation(new() { source = _source, elementId = id, key = kvp.Key, value = kvp.Value, isRemove = false }),
                onRemove: (id, kvp) => operand.EnqueuePendingOperation(new() { source = _source, elementId = id, key = kvp.Key, value = kvp.Value, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IReadOnlyList<DictionaryOp<TKey, TValue>> GetInitializationOperations()
        {
            var initOperations = new List<DictionaryOp<TKey, TValue>>();
            var subscription = _source.SubscribeWithId((id, kvp) => initOperations.Add(new DictionaryOp<TKey, TValue>() { source = _source, elementId = id, key = kvp.Key, value = kvp.Value }));
            subscription.Dispose();
            return initOperations;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }

    public class SetOperationStreamObservable<T> : IInitializationOperationsProvider<SetOp<T>>, IDisposable
    {
        private ISetObservable<T> _source;
        private IObservableOperand<SetOp<T>> _operand;
        private IDisposable _subscriptions;

        public SetOperationStreamObservable(ISetObservable<T> source, IObservableOperand<SetOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) => operand.EnqueuePendingOperation(new() { source = _source, elementId = id, element = element, isRemove = false }),
                onRemove: (id, element) => operand.EnqueuePendingOperation(new() { source = _source, elementId = id, element = element, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IReadOnlyList<SetOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<SetOp<T>>();
            var subscription = _source.SubscribeWithId((id, element) => initOperations.Add(new SetOp<T>() { source = _source, elementId = id, element = element }));
            subscription.Dispose();
            return initOperations;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.OnDisposed();
        }
    }
}