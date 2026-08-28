using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ValueOperationStreamObservable<T> : IInitializationOperationsProvider<ValueOp<T>>, IDisposable
    {
        private IValueObservable<T> _source;
        private Observable<ValueOp<T>> _operand;
        private IDisposable _subscriptions;

        public ValueOperationStreamObservable(IValueObservable<T> source, Observable<ValueOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.Subscribe(
                onNext: x => operand.EnqueueOperation(new() { source = _source, value = x }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<ValueOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<ValueOp<T>>();
            var subscription = _source.Subscribe(x => initOperations.Add(new ValueOp<T>() { source = _source, value = x }));
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
        private Observable<CollectionOp<T>> _operand;
        private IDisposable _subscriptions;

        public CollectionOperationStreamObservable(ICollectionObservable<T> source, Observable<CollectionOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) => operand.EnqueueOperation(new() { source = _source, elementId = id, element = element, isRemove = false }),
                onRemove: (id, element) => operand.EnqueueOperation(new() { source = _source, elementId = id, element = element, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<CollectionOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<CollectionOp<T>>();
            var subscription = _source.SubscribeWithId((id, element) => initOperations.Add(new CollectionOp<T>() { source = _source, elementId = id, element = element }));
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
        private Observable<ListOp<T>> _operand;
        private IDisposable _subscriptions;

        public ListOperationStreamObservable(IListObservable<T> source, Observable<ListOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, index, element) => operand.EnqueueOperation(new() { source = _source, elementId = id, index = index, element = element, isRemove = false }),
                onRemove: (id, index, element) => operand.EnqueueOperation(new() { source = _source, elementId = id, index = index, element = element, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<ListOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<ListOp<T>>();
            var subscription = _source.SubscribeWithId((id, index, element) => initOperations.Add(new ListOp<T>() { source = _source, elementId = id, index = index, element = element }));
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
        private Observable<DictionaryOp<TKey, TValue>> _operand;
        private IDisposable _subscriptions;

        public DictionaryOperationStreamObservable(IDictionaryObservable<TKey, TValue> source, Observable<DictionaryOp<TKey, TValue>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, kvp) => operand.EnqueueOperation(new() { source = _source, elementId = id, key = kvp.Key, value = kvp.Value, isRemove = false }),
                onRemove: (id, kvp) => operand.EnqueueOperation(new() { source = _source, elementId = id, key = kvp.Key, value = kvp.Value, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<DictionaryOp<TKey, TValue>> GetInitializationOperations()
        {
            var initOperations = new List<DictionaryOp<TKey, TValue>>();
            var subscription = _source.SubscribeWithId((id, kvp) => initOperations.Add(new DictionaryOp<TKey, TValue>() { source = _source, elementId = id, key = kvp.Key, value = kvp.Value }));
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
        private Observable<SetOp<T>> _operand;
        private IDisposable _subscriptions;

        public SetOperationStreamObservable(ISetObservable<T> source, Observable<SetOp<T>> operand)
        {
            _source = source;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) => operand.EnqueueOperation(new() { source = _source, elementId = id, element = element, isRemove = false }),
                onRemove: (id, element) => operand.EnqueueOperation(new() { source = _source, elementId = id, element = element, isRemove = true }),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<SetOp<T>> GetInitializationOperations()
        {
            var initOperations = new List<SetOp<T>>();
            var subscription = _source.SubscribeWithId((id, element) => initOperations.Add(new SetOp<T>() { source = _source, elementId = id, element = element }));
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