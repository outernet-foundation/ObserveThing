using System;
using System.Collections.Generic;
using System.Linq;
using FofX.Stateful;

namespace ObserveThing.StatefulExtensions
{
    public static class Extensions
    {
        public static ICollectionObservable<PrimitiveMapPair<TLeft, TRight>> AsObservable<TLeft, TRight>(this ObservablePrimitiveMap<TLeft, TRight> map)
            => new FactoryCollectionObservable<PrimitiveMapPair<TLeft, TRight>>(receiver => new StatefulPrimitiveMapObservable<TLeft, TRight>(map, receiver));

        public class StatefulPrimitiveMapObservable<TLeft, TRight> : IDisposable
        {
            private ObservablePrimitiveMap<TLeft, TRight> _primitiveMap;
            private ICollectionObserver<PrimitiveMapPair<TLeft, TRight>> _receiver;
            private CollectionIdProvider _idProvider;
            private Dictionary<PrimitiveMapPair<TLeft, TRight>, uint> _idByElement = new Dictionary<PrimitiveMapPair<TLeft, TRight>, uint>();
            private bool _disposed;

            public StatefulPrimitiveMapObservable(ObservablePrimitiveMap<TLeft, TRight> primitiveMap, ICollectionObserver<PrimitiveMapPair<TLeft, TRight>> receiver)
            {
                _primitiveMap = primitiveMap;
                _receiver = receiver;
                _idProvider = new CollectionIdProvider(x => _idByElement.Values.Contains(x));
                _primitiveMap.context.RegisterObserver(HandlePrimitiveMapChanged, new ObserverParameters() { scope = ObservationScope.Self }, _primitiveMap);
            }

            private void HandlePrimitiveMapChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                {
                    foreach (var element in _primitiveMap)
                    {
                        var id = _idProvider.GetUnusedId();
                        _idByElement.Add(element, id);
                        _receiver.OnAdd(id, element);
                    }

                    return;
                }

                foreach (var change in args.changes)
                {
                    if (change.changeType == ChangeType.Add)
                    {
                        var element = (PrimitiveMapPair<TLeft, TRight>)change.collectionElement;
                        var id = _idProvider.GetUnusedId();
                        _idByElement.Add(element, id);
                        _receiver.OnAdd(id, element);
                    }
                    else if (change.changeType == ChangeType.Remove)
                    {
                        var element = (PrimitiveMapPair<TLeft, TRight>)change.collectionElement;
                        var id = _idByElement[element];
                        _idByElement.Remove(element);
                        _receiver.OnRemove(id, element);
                    }
                    else if (change.changeType == ChangeType.Dispose)
                    {
                        Dispose();
                        return;
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _primitiveMap.context.DeregisterObserver(HandlePrimitiveMapChanged);
                _receiver.OnDispose();
            }
        }

        public static IValueObservable<IReadOnlyCollection<T>> AsObservable<T>(this ObservablePrimitiveArray<T> primitiveArray)
            => new FactoryValueObservable<IReadOnlyCollection<T>>(receiver => new StatefulPrimitiveArrayObservable<T>(primitiveArray, receiver));

        private class StatefulPrimitiveArrayObservable<T> : IDisposable
        {
            private ObservablePrimitiveArray<T> _primitiveArray;
            private IValueObserver<IReadOnlyCollection<T>> _receiver;
            private bool _disposed;

            public StatefulPrimitiveArrayObservable(ObservablePrimitiveArray<T> primitiveArray, IValueObserver<IReadOnlyCollection<T>> receiver)
            {
                _primitiveArray = primitiveArray;
                _receiver = receiver;

                _primitiveArray.context.RegisterObserver(HandlePrimitiveChanged, new ObserverParameters() { scope = ObservationScope.Self }, _primitiveArray);
            }

            private void HandlePrimitiveChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                {
                    _receiver.OnNext(_primitiveArray.ToArray());
                    return;
                }

                foreach (var change in args.changes)
                {
                    if (change.changeType == ChangeType.Set)
                    {
                        _receiver.OnNext((IReadOnlyCollection<T>)change.currentValue);
                    }
                    else if (change.changeType == ChangeType.Dispose)
                    {
                        Dispose();
                        return;
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _primitiveArray.context.DeregisterObserver(HandlePrimitiveChanged);
                _receiver.OnDispose();
            }
        }

        public static IDictionaryObservable<TKey, TValue> AsObservable<TKey, TValue>(this ObservableDictionary<TKey, TValue> dictionary)
            where TValue : IObservableNode, new() => new FactoryDictionaryObservable<TKey, TValue>(receiver => new StatefulDictionaryObservable<TKey, TValue>(dictionary, receiver));

        public class StatefulDictionaryObservable<TKey, TValue> : IDisposable where TValue : IObservableNode, new()
        {
            private ObservableDictionary<TKey, TValue> _dictionary;
            private IDictionaryObserver<TKey, TValue> _receiver;
            private CollectionIdProvider _idProvider;
            private Dictionary<TKey, uint> _idByKey = new Dictionary<TKey, uint>();
            private bool _disposed;

            public StatefulDictionaryObservable(ObservableDictionary<TKey, TValue> dictionary, IDictionaryObserver<TKey, TValue> receiver)
            {
                _dictionary = dictionary;
                _receiver = receiver;
                _idProvider = new CollectionIdProvider(x => _idByKey.Values.Contains(x));
                _dictionary.context.RegisterObserver(HandleDictionaryChanged, new ObserverParameters() { scope = ObservationScope.Self }, _dictionary);
            }

            private void HandleDictionaryChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                {
                    foreach (var kvp in _dictionary)
                    {
                        var id = _idProvider.GetUnusedId();
                        _idByKey.Add(kvp.key, id);
                        _receiver.OnAdd(id, new KeyValuePair<TKey, TValue>(kvp.key, kvp.value));
                    }

                    return;
                }

                foreach (var change in args.changes)
                {
                    if (change.changeType == ChangeType.Add)
                    {
                        var kvp = (KVP<TKey, TValue>)change.collectionElement;
                        var id = _idProvider.GetUnusedId();
                        _idByKey.Add(kvp.key, id);
                        _receiver.OnAdd(id, new KeyValuePair<TKey, TValue>(kvp.key, kvp.value));
                    }
                    else if (change.changeType == ChangeType.Remove)
                    {
                        var kvp = (KVP<TKey, TValue>)change.collectionElement;
                        var id = _idByKey[kvp.key];
                        _idByKey.Remove(kvp.key);
                        _receiver.OnRemove(id, new KeyValuePair<TKey, TValue>(kvp.key, kvp.value));
                    }
                    else if (change.changeType == ChangeType.Dispose)
                    {
                        Dispose();
                        return;
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _dictionary.context.DeregisterObserver(HandleDictionaryChanged);
                _receiver.OnDispose();
            }
        }

        public static IListObservable<T> AsObservable<T>(this ObservableList<T> list)
            where T : IObservableNode, new() => new FactoryListObservable<T>(receiver => new StatefulListObservable<T>(list, receiver));

        public class StatefulListObservable<T> : IDisposable where T : IObservableNode, new()
        {
            private ObservableList<T> _list;
            private IListObserver<T> _receiver;
            private CollectionIdProvider _idProvider;
            private Dictionary<T, uint> _idByElement = new Dictionary<T, uint>();
            private bool _disposed;

            public StatefulListObservable(ObservableList<T> list, IListObserver<T> receiver)
            {
                _list = list;
                _receiver = receiver;
                _idProvider = new CollectionIdProvider(x => _idByElement.Values.Contains(x));
                _list.context.RegisterObserver(HandleListChanged, new ObserverParameters() { scope = ObservationScope.Self }, _list);
            }

            private void HandleListChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                {
                    for (int i = 0; i < _list.count; i++)
                    {
                        var element = _list[i];
                        var id = _idProvider.GetUnusedId();
                        _idByElement.Add(element, id);
                        _receiver.OnAdd(id, i, element);
                    }

                    return;
                }

                foreach (var change in args.changes)
                {
                    if (change.changeType == ChangeType.Add)
                    {
                        var element = (T)change.collectionElement;
                        var id = _idProvider.GetUnusedId();
                        _idByElement.Add(element, id);
                        _receiver.OnAdd(id, change.index.Value, element);
                    }
                    else if (change.changeType == ChangeType.Remove)
                    {
                        var element = (T)change.collectionElement;
                        var id = _idByElement[element];
                        _idByElement.Remove(element);
                        _receiver.OnRemove(id, change.index.Value, element);
                    }
                    else if (change.changeType == ChangeType.Dispose)
                    {
                        Dispose();
                        return;
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _list.context.DeregisterObserver(HandleListChanged);
                _receiver.OnDispose();
            }
        }

        public static IListObservable<object> AsObservable(this IObservableList list)
            => new FactoryListObservable<object>(receiver => new StatefulListObservable(list, receiver));

        public class StatefulListObservable : IDisposable
        {
            private IObservableList _list;
            private IListObserver<object> _receiver;
            private CollectionIdProvider _idProvider;
            private Dictionary<object, uint> _idByElement = new Dictionary<object, uint>();
            private bool _disposed;

            public StatefulListObservable(IObservableList list, IListObserver<object> receiver)
            {
                _list = list;
                _receiver = receiver;
                _idProvider = new CollectionIdProvider(x => _idByElement.Values.Contains(x));
                _list.context.RegisterObserver(HandleListChanged, new ObserverParameters() { scope = ObservationScope.Self }, _list);
            }

            private void HandleListChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                {
                    for (int i = 0; i < _list.count; i++)
                    {
                        var element = _list[i];
                        var id = _idProvider.GetUnusedId();
                        _idByElement.Add(element, id);
                        _receiver.OnAdd(id, i, element);
                    }

                    return;
                }

                foreach (var change in args.changes)
                {
                    if (change.changeType == ChangeType.Add)
                    {
                        var id = _idProvider.GetUnusedId();
                        _idByElement.Add(change.collectionElement, id);
                        _receiver.OnAdd(id, change.index.Value, change.collectionElement);
                    }
                    else if (change.changeType == ChangeType.Remove)
                    {
                        var id = _idByElement[change.collectionElement];
                        _idByElement.Remove(change.collectionElement);
                        _receiver.OnRemove(id, change.index.Value, change.collectionElement);
                    }
                    else if (change.changeType == ChangeType.Dispose)
                    {
                        Dispose();
                        return;
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _list.context.DeregisterObserver(HandleListChanged);
                _receiver.OnDispose();
            }
        }

        public static ICollectionObservable<T> AsObservable<T>(this ObservableSet<T> set)
            => new FactoryCollectionObservable<T>(receiver => new StatefulSetObservable<T>(set, receiver));

        public class StatefulSetObservable<T> : IDisposable
        {
            private ObservableSet<T> _set;
            private ICollectionObserver<T> _receiver;
            private CollectionIdProvider _idProvider;
            private Dictionary<T, uint> _idByElement = new Dictionary<T, uint>();
            private bool _disposed;

            public StatefulSetObservable(ObservableSet<T> set, ICollectionObserver<T> receiver)
            {
                _set = set;
                _receiver = receiver;
                _idProvider = new CollectionIdProvider(x => _idByElement.Values.Contains(x));
                _set.context.RegisterObserver(HandleSetChanged, new ObserverParameters() { scope = ObservationScope.Self }, _set);
            }

            private void HandleSetChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                {
                    foreach (var element in _set)
                    {
                        var id = _idProvider.GetUnusedId();
                        _idByElement.Add(element, id);
                        _receiver.OnAdd(id, element);
                    }

                    return;
                }

                foreach (var change in args.changes)
                {
                    if (change.changeType == ChangeType.Add)
                    {
                        var element = (T)change.collectionElement;
                        var id = _idProvider.GetUnusedId();
                        _idByElement.Add(element, id);
                        _receiver.OnAdd(id, element);
                    }
                    else if (change.changeType == ChangeType.Remove)
                    {
                        var element = (T)change.collectionElement;
                        var id = _idByElement[element];
                        _idByElement.Remove(element);
                        _receiver.OnRemove(id, element);
                    }
                    else if (change.changeType == ChangeType.Dispose)
                    {
                        Dispose();
                        return;
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _set.context.DeregisterObserver(HandleSetChanged);
                _receiver.OnDispose();
            }
        }

        public static IValueObservable<T> AsObservable<T>(this ObservablePrimitive<T> primitive)
            => new FactoryValueObservable<T>(receiver => new StatefulPrimitiveObservable<T>(primitive, receiver));

        public class StatefulPrimitiveObservable<T> : IDisposable
        {
            private ObservablePrimitive<T> _primitive;
            private IValueObserver<T> _receiver;
            private bool _disposed;

            public StatefulPrimitiveObservable(ObservablePrimitive<T> primitive, IValueObserver<T> receiver)
            {
                _primitive = primitive;
                _receiver = receiver;

                _primitive.context.RegisterObserver(HandlePrimitiveChanged, new ObserverParameters() { scope = ObservationScope.Self }, _primitive);
            }

            private void HandlePrimitiveChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                {
                    _receiver.OnNext(_primitive.value);
                    return;
                }

                foreach (var change in args.changes)
                {
                    if (change.changeType == ChangeType.Set)
                    {
                        _receiver.OnNext((T)change.currentValue);
                    }
                    else if (change.changeType == ChangeType.Dispose)
                    {
                        Dispose();
                        return;
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _primitive.context.DeregisterObserver(HandlePrimitiveChanged);
                _receiver.OnDispose();
            }
        }

        public static IValueObservable<object> AsObservable(this IObservablePrimitive primitive)
            => new FactoryValueObservable<object>(receiver => new StatefulPrimitiveObservable(primitive, receiver));

        public class StatefulPrimitiveObservable : IDisposable
        {
            private IObservablePrimitive _primitive;
            private IValueObserver<object> _receiver;
            private bool _disposed;

            public StatefulPrimitiveObservable(IObservablePrimitive primitive, IValueObserver<object> receiver)
            {
                _primitive = primitive;
                _receiver = receiver;

                _primitive.context.RegisterObserver(HandlePrimitiveChanged, new ObserverParameters() { scope = ObservationScope.Self }, _primitive);
            }

            private void HandlePrimitiveChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                {
                    _receiver.OnNext(_primitive.GetValue());
                    return;
                }

                foreach (var change in args.changes)
                {
                    if (change.changeType == ChangeType.Set)
                    {
                        _receiver.OnNext(change.currentValue);
                    }
                    else if (change.changeType == ChangeType.Dispose)
                    {
                        Dispose();
                        return;
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _primitive.context.DeregisterObserver(HandlePrimitiveChanged);
                _receiver.OnDispose();
            }
        }
    }
}