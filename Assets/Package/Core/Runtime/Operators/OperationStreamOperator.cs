using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ValueEventStreamOperator<T, TOperation> : IInitializationOperationsProvider<TOperation>, IDisposable
    {
        private IValueObservable<T> _source;
        private Func<T, TOperation> _constructOperation;
        private ObservableEventStream<TOperation> _operand;
        private IDisposable _subscriptions;

        public ValueEventStreamOperator(IValueObservable<T> source, Func<T, TOperation> constructOperation, ObservableEventStream<TOperation> operand)
        {
            _source = source;
            _constructOperation = constructOperation;
            _operand = operand;

            _subscriptions = _source.Subscribe(
                onNext: x => operand.EnqueueOperation(_constructOperation(x)),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<TOperation> GetInitializationOperations()
        {
            var initOperations = new List<TOperation>();
            var subscription = _source.Subscribe(x => initOperations.Add(_constructOperation(x)));
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

    public class CollectionEventStreamOperator<T, TOperation> : IInitializationOperationsProvider<TOperation>, IDisposable
    {
        private ICollectionObservable<T> _source;
        private Func<uint, T, TOperation> _constructAddOperation;
        private Func<uint, T, TOperation> _constructRemoveOperation;
        private ObservableEventStream<TOperation> _operand;
        private IDisposable _subscriptions;

        public CollectionEventStreamOperator(ICollectionObservable<T> source, Func<uint, T, TOperation> constructAddOperation, Func<uint, T, TOperation> constructRemoveOperation, ObservableEventStream<TOperation> operand)
        {
            _source = source;
            _constructAddOperation = constructAddOperation;
            _constructRemoveOperation = constructRemoveOperation;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) => operand.EnqueueOperation(_constructAddOperation(id, element)),
                onRemove: (id, element) => operand.EnqueueOperation(_constructRemoveOperation(id, element)),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<TOperation> GetInitializationOperations()
        {
            var initOperations = new List<TOperation>();
            var subscription = _source.SubscribeWithId((id, element) => initOperations.Add(_constructAddOperation(id, element)));
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

    public class ListEventStreamOperator<T, TOperation> : IInitializationOperationsProvider<TOperation>, IDisposable
    {
        private IListObservable<T> _source;
        private Func<uint, int, T, TOperation> _constructAddOperation;
        private Func<uint, int, T, TOperation> _constructRemoveOperation;
        private ObservableEventStream<TOperation> _operand;
        private IDisposable _subscriptions;

        public ListEventStreamOperator(IListObservable<T> source, Func<uint, int, T, TOperation> constructAddOperation, Func<uint, int, T, TOperation> constructRemoveOperation, ObservableEventStream<TOperation> operand)
        {
            _source = source;
            _constructAddOperation = constructAddOperation;
            _constructRemoveOperation = constructRemoveOperation;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, index, element) => operand.EnqueueOperation(_constructAddOperation(id, index, element)),
                onRemove: (id, index, element) => operand.EnqueueOperation(_constructRemoveOperation(id, index, element)),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<TOperation> GetInitializationOperations()
        {
            var initOperations = new List<TOperation>();
            var subscription = _source.SubscribeWithId((id, index, element) => initOperations.Add(_constructAddOperation(id, index, element)));
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

    public class DictionaryEventStreamOperator<TKey, TValue, TOperation> : IInitializationOperationsProvider<TOperation>, IDisposable
    {
        private IDictionaryObservable<TKey, TValue> _source;
        private Func<uint, KeyValuePair<TKey, TValue>, TOperation> _constructAddOperation;
        private Func<uint, KeyValuePair<TKey, TValue>, TOperation> _constructRemoveOperation;
        private ObservableEventStream<TOperation> _operand;
        private IDisposable _subscriptions;

        public DictionaryEventStreamOperator(IDictionaryObservable<TKey, TValue> source, Func<uint, KeyValuePair<TKey, TValue>, TOperation> constructAddOperation, Func<uint, KeyValuePair<TKey, TValue>, TOperation> constructRemoveOperation, ObservableEventStream<TOperation> operand)
        {
            _source = source;
            _constructAddOperation = constructAddOperation;
            _constructRemoveOperation = constructRemoveOperation;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, kvp) => operand.EnqueueOperation(_constructAddOperation(id, kvp)),
                onRemove: (id, kvp) => operand.EnqueueOperation(_constructRemoveOperation(id, kvp)),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<TOperation> GetInitializationOperations()
        {
            var initOperations = new List<TOperation>();
            var subscription = _source.SubscribeWithId((id, kvp) => initOperations.Add(_constructAddOperation(id, kvp)));
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

    public class SetEventStreamOperator<T, TOperation> : IInitializationOperationsProvider<TOperation>, IDisposable
    {
        private ISetObservable<T> _source;
        private Func<uint, T, TOperation> _constructAddOperation;
        private Func<uint, T, TOperation> _constructRemoveOperation;
        private ObservableEventStream<TOperation> _operand;
        private IDisposable _subscriptions;

        public SetEventStreamOperator(ISetObservable<T> source, Func<uint, T, TOperation> constructAddOperation, Func<uint, T, TOperation> constructRemoveOperation, ObservableEventStream<TOperation> operand)
        {
            _source = source;
            _constructAddOperation = constructAddOperation;
            _constructRemoveOperation = constructRemoveOperation;
            _operand = operand;

            _subscriptions = _source.SubscribeWithId(
                onAdd: (id, element) => operand.EnqueueOperation(_constructAddOperation(id, element)),
                onRemove: (id, element) => operand.EnqueueOperation(_constructRemoveOperation(id, element)),
                onDispose: Dispose,
                onError: operand.OnError,
                immediate: true
            );
        }

        public IEnumerable<TOperation> GetInitializationOperations()
        {
            var initOperations = new List<TOperation>();
            var subscription = _source.SubscribeWithId((id, element) => initOperations.Add(_constructAddOperation(id, element)));
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