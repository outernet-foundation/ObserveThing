using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ValueOperationStreamObservable<T> : IInitializationOperationsProvider<ValueOp<T>>, IDisposable
    {
        private IValueObservable<T> _source;
        private OpStreamObservable<ValueOp<T>> _operand;
        private IDisposable _subscriptions;

        public ValueOperationStreamObservable(IValueObservable<T> source, OpStreamObservable<ValueOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.Subscribe(
                onNext: x => operand.EnqueueOperation(new() { value = x }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<ValueOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<ValueOp<T>>();
            var subscription = _source.Subscribe(x => initOperations.Add(new ValueOp<T>() { value = x }));
            subscription.Dispose();

            foreach (var op in initOperations)
                yield return op;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }

    public class CollectionOperationStreamObservable<T> : IInitializationOperationsProvider<CollectionOp<T>>, IDisposable
    {
        private ICollectionObservable<T> _source;
        private OpStreamObservable<CollectionOp<T>> _operand;
        private IDisposable _subscriptions;

        public CollectionOperationStreamObservable(ICollectionObservable<T> source, OpStreamObservable<CollectionOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) => operand.EnqueueOperation(new() { elementId = id, element = element, isRemove = false }),
                onRemove: (id, element) => operand.EnqueueOperation(new() { elementId = id, element = element, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<CollectionOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<CollectionOp<T>>();
            var subscription = _source.SubscribeWithId((id, element) => initOperations.Add(new CollectionOp<T>() { elementId = id, element = element }));
            subscription.Dispose();

            foreach (var op in initOperations)
                yield return op;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }

    public class ListOperationStreamObservable<T> : IInitializationOperationsProvider<ListOp<T>>, IDisposable
    {
        private IListObservable<T> _source;
        private OpStreamObservable<ListOp<T>> _operand;
        private IDisposable _subscriptions;

        public ListOperationStreamObservable(IListObservable<T> source, OpStreamObservable<ListOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, index, element) => operand.EnqueueOperation(new() { elementId = id, index = index, element = element, isRemove = false }),
                onRemove: (id, index, element) => operand.EnqueueOperation(new() { elementId = id, index = index, element = element, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<ListOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<ListOp<T>>();
            var subscription = _source.SubscribeWithId((id, index, element) => initOperations.Add(new ListOp<T>() { elementId = id, index = index, element = element }));
            subscription.Dispose();

            foreach (var op in initOperations)
                yield return op;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }

    public class DictionaryOperationStreamObservable<TKey, TValue> : IInitializationOperationsProvider<DictionaryOp<TKey, TValue>>, IDisposable
    {
        private IDictionaryObservable<TKey, TValue> _source;
        private OpStreamObservable<DictionaryOp<TKey, TValue>> _operand;
        private IDisposable _subscriptions;

        public DictionaryOperationStreamObservable(IDictionaryObservable<TKey, TValue> source, OpStreamObservable<DictionaryOp<TKey, TValue>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, kvp) => operand.EnqueueOperation(new() { elementId = id, key = kvp.Key, value = kvp.Value, isRemove = false }),
                onRemove: (id, kvp) => operand.EnqueueOperation(new() { elementId = id, key = kvp.Key, value = kvp.Value, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<DictionaryOp<TKey, TValue>> GetInitializationOperations()
        {
            var initOperations = new List<DictionaryOp<TKey, TValue>>();
            var subscription = _source.SubscribeWithId((id, kvp) => initOperations.Add(new DictionaryOp<TKey, TValue>() { elementId = id, key = kvp.Key, value = kvp.Value }));
            subscription.Dispose();

            foreach (var op in initOperations)
                yield return op;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }

    public class SetOperationStreamObservable<T> : IInitializationOperationsProvider<SetOp<T>>, IDisposable
    {
        private ISetObservable<T> _source;
        private OpStreamObservable<SetOp<T>> _operand;
        private IDisposable _subscriptions;

        public SetOperationStreamObservable(ISetObservable<T> source, OpStreamObservable<SetOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) => operand.EnqueueOperation(new() { elementId = id, element = element, isRemove = false }),
                onRemove: (id, element) => operand.EnqueueOperation(new() { elementId = id, element = element, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<SetOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<SetOp<T>>();
            var subscription = _source.SubscribeWithId((id, element) => initOperations.Add(new SetOp<T>() { elementId = id, element = element }));
            subscription.Dispose();

            foreach (var op in initOperations)
                yield return op;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }
}