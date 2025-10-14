using System;
using System.Collections.Generic;
using System.Linq;
using FofX.Stateful;

namespace ObserveThing.StatefulExtensions
{
    public static class Extensions
    {
        public static ICollectionObservable<PrimitiveMapPair<TLeft, TRight>> AsObservable<TLeft, TRight>(this ObservablePrimitiveMap<TLeft, TRight> map)
        {
            return new StatefulPrimitiveMapObservable<TLeft, TRight>(map);
        }

        private class StatefulPrimitiveMapObservable<TLeft, TRight> : ICollectionObservable<PrimitiveMapPair<TLeft, TRight>>
        {
            private ObservablePrimitiveMap<TLeft, TRight> _source;
            private CollectionEventArgs<PrimitiveMapPair<TLeft, TRight>> _args;
            private List<IObserver<ICollectionEventArgs<PrimitiveMapPair<TLeft, TRight>>>> _observers = new List<IObserver<ICollectionEventArgs<PrimitiveMapPair<TLeft, TRight>>>>();

            public StatefulPrimitiveMapObservable(ObservablePrimitiveMap<TLeft, TRight> source)
            {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<ICollectionEventArgs<PrimitiveMapPair<TLeft, TRight>>> observer)
            {
                _observers.Add(observer);

                if (_observers.Count == 1)
                    _source.context.RegisterObserver(HandleSourceChanged, new ObserverParameters() { scope = ObservationScope.Self }, _source);

                _args.operationType = OpType.Add;

                foreach (var element in _source)
                {
                    _args.element = element;
                    observer.OnNext(_args);
                }

                return new ObserverHandle() { observer = observer, source = this };
            }

            private void HandleSourceChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                    return;

                foreach (var change in args.changes)
                {
                    switch (change.changeType)
                    {
                        case ChangeType.Add:
                            _args.operationType = OpType.Add;
                            _args.element = (PrimitiveMapPair<TLeft, TRight>)change.collectionElement;
                            NotifyObservers(_args);
                            break;

                        case ChangeType.Remove:
                            _args.operationType = OpType.Remove;
                            _args.element = (PrimitiveMapPair<TLeft, TRight>)change.collectionElement;
                            NotifyObservers(_args);
                            break;

                        case ChangeType.Dispose:
                            DisposeObservers();
                            return;
                    }
                }
            }

            private void NotifyObservers(ICollectionEventArgs<PrimitiveMapPair<TLeft, TRight>> args)
            {
                foreach (var observer in _observers)
                    observer.OnNext(args);
            }

            private void DisposeObservers()
            {
                foreach (var observer in _observers)
                    observer.OnDispose();

                _observers.Clear();
            }

            private void Unsubscribe(IObserver<ICollectionEventArgs<PrimitiveMapPair<TLeft, TRight>>> observer)
            {
                if (_observers.Remove(observer) && _observers.Count == 0)
                    _source.context.DeregisterObserver(HandleSourceChanged);
            }

            private class ObserverHandle : IDisposable
            {
                public IObserver<ICollectionEventArgs<PrimitiveMapPair<TLeft, TRight>>> observer;
                public StatefulPrimitiveMapObservable<TLeft, TRight> source;

                private bool _disposed;

                public void Dispose()
                {
                    if (_disposed)
                        return;

                    _disposed = true;
                    source.Unsubscribe(observer);
                }
            }
        }

        public static IValueObservable<T[]> AsObservable<T>(this ObservablePrimitiveArray<T> primitiveArray)
        {
            return new StatefulPrimitiveArrayObservable<T>(primitiveArray);
        }

        private class StatefulPrimitiveArrayObservable<T> : IValueObservable<T[]>
        {
            private ObservablePrimitiveArray<T> _source;
            private T[] _previousValue;
            private ValueEventArgs<T[]> _args;
            private List<IObserver<IValueEventArgs<T[]>>> _observers = new List<IObserver<IValueEventArgs<T[]>>>();

            public StatefulPrimitiveArrayObservable(ObservablePrimitiveArray<T> source)
            {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<IValueEventArgs<T[]>> observer)
            {
                _observers.Add(observer);

                if (_observers.Count == 1)
                    _source.context.RegisterObserver(HandleSourceChanged, new ObserverParameters() { scope = ObservationScope.Self }, _source);

                _args.previousValue = null;
                _args.currentValue = _source.ToArray();

                observer.OnNext(_args);

                return new ObserverHandle() { observer = observer, source = this };
            }

            private void HandleSourceChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                    return;

                if (_source.disposed)
                {
                    foreach (var observer in _observers)
                        observer.OnDispose();

                    _observers.Clear();

                    return;
                }

                _args.previousValue = _previousValue;
                _args.currentValue = _source.ToArray();

                foreach (var observer in _observers)
                    observer.OnNext(_args);

                _previousValue = _args.currentValue;
            }

            private void Unsubscribe(IObserver<IValueEventArgs<T[]>> observer)
            {
                if (_observers.Remove(observer) && _observers.Count == 0)
                    _source.context.DeregisterObserver(HandleSourceChanged);
            }

            private class ObserverHandle : IDisposable
            {
                public IObserver<IValueEventArgs<T[]>> observer;
                public StatefulPrimitiveArrayObservable<T> source;

                private bool _disposed;

                public void Dispose()
                {
                    if (_disposed)
                        return;

                    _disposed = true;
                    source.Unsubscribe(observer);
                }
            }
        }

        public static IDictionaryObservable<TKey, TValue> AsObservable<TKey, TValue>(this ObservableDictionary<TKey, TValue> dictionary)
            where TValue : IObservableNode, new()
        {
            return new StatefulDictionaryObservable<TKey, TValue>(dictionary);
        }

        private class StatefulDictionaryObservable<TKey, TValue> : IDictionaryObservable<TKey, TValue> where TValue : IObservableNode, new()
        {
            private ObservableDictionary<TKey, TValue> _source;
            private DictionaryEventArgs<TKey, TValue> _args;
            private List<IObserver<IDictionaryEventArgs<TKey, TValue>>> _observers = new List<IObserver<IDictionaryEventArgs<TKey, TValue>>>();

            public StatefulDictionaryObservable(ObservableDictionary<TKey, TValue> source)
            {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<IDictionaryEventArgs<TKey, TValue>> observer)
            {
                _observers.Add(observer);

                if (_observers.Count == 1)
                    _source.context.RegisterObserver(HandleSourceChanged, new ObserverParameters() { scope = ObservationScope.Self }, _source);

                _args.operationType = OpType.Add;

                foreach (var kvp in _source)
                {
                    _args.element = new KeyValuePair<TKey, TValue>(kvp.key, kvp.value);
                    observer.OnNext(_args);
                }

                return new ObserverHandle() { observer = observer, source = this };
            }

            private void HandleSourceChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                    return;

                foreach (var change in args.changes)
                {
                    switch (change.changeType)
                    {
                        case ChangeType.Add:
                            _args.operationType = OpType.Add;
                            var addedElement = (KVP<TKey, TValue>)change.collectionElement;
                            _args.element = new KeyValuePair<TKey, TValue>(addedElement.key, addedElement.value);
                            NotifyObservers(_args);
                            break;

                        case ChangeType.Remove:
                            _args.operationType = OpType.Remove;
                            var removedElement = (KVP<TKey, TValue>)change.collectionElement;
                            _args.element = new KeyValuePair<TKey, TValue>(removedElement.key, removedElement.value);
                            NotifyObservers(_args);
                            break;

                        case ChangeType.Dispose:
                            DisposeObservers();
                            return;
                    }
                }
            }

            private void NotifyObservers(IDictionaryEventArgs<TKey, TValue> args)
            {
                foreach (var observer in _observers)
                    observer.OnNext(args);
            }

            private void DisposeObservers()
            {
                foreach (var observer in _observers)
                    observer.OnDispose();

                _observers.Clear();
            }

            private void Unsubscribe(IObserver<IDictionaryEventArgs<TKey, TValue>> observer)
            {
                if (_observers.Remove(observer) && _observers.Count == 0)
                    _source.context.DeregisterObserver(HandleSourceChanged);
            }

            private class ObserverHandle : IDisposable
            {
                public IObserver<IDictionaryEventArgs<TKey, TValue>> observer;
                public StatefulDictionaryObservable<TKey, TValue> source;

                private bool _disposed;

                public void Dispose()
                {
                    if (_disposed)
                        return;

                    _disposed = true;
                    source.Unsubscribe(observer);
                }
            }
        }

        public static IListObservable<T> AsObservable<T>(this ObservableList<T> list)
            where T : IObservableNode, new()
        {
            return new StatefulListObservable<T>(list);
        }

        private class StatefulListObservable<T> : IListObservable<T> where T : IObservableNode, new()
        {
            private ObservableList<T> _source;
            private ListEventArgs<T> _args;
            private List<IObserver<IListEventArgs<T>>> _observers = new List<IObserver<IListEventArgs<T>>>();

            public StatefulListObservable(ObservableList<T> source)
            {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<IListEventArgs<T>> observer)
            {
                _observers.Add(observer);

                if (_observers.Count == 1)
                    _source.context.RegisterObserver(HandleSourceChanged, new ObserverParameters() { scope = ObservationScope.Self }, _source);

                _args.operationType = OpType.Add;

                for (int i = 0; i < _source.count; i++)
                {
                    _args.index = i;
                    _args.element = _source[i];
                    observer.OnNext(_args);
                }

                return new ObserverHandle() { observer = observer, source = this };
            }

            private void HandleSourceChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                    return;

                foreach (var change in args.changes)
                {
                    switch (change.changeType)
                    {
                        case ChangeType.Add:
                            _args.operationType = OpType.Add;
                            _args.index = change.index.Value;
                            _args.element = (T)change.collectionElement;
                            NotifyObservers(_args);
                            break;

                        case ChangeType.Remove:
                            _args.operationType = OpType.Remove;
                            _args.index = change.index.Value;
                            _args.element = (T)change.collectionElement;
                            NotifyObservers(_args);
                            break;

                        case ChangeType.Dispose:
                            DisposeObservers();
                            return;
                    }
                }
            }

            private void NotifyObservers(IListEventArgs<T> args)
            {
                foreach (var observer in _observers)
                    observer.OnNext(args);
            }

            private void DisposeObservers()
            {
                foreach (var observer in _observers)
                    observer.OnDispose();

                _observers.Clear();
            }

            private void Unsubscribe(IObserver<IListEventArgs<T>> observer)
            {
                if (_observers.Remove(observer) && _observers.Count == 0)
                    _source.context.DeregisterObserver(HandleSourceChanged);
            }

            private class ObserverHandle : IDisposable
            {
                public IObserver<IListEventArgs<T>> observer;
                public StatefulListObservable<T> source;

                private bool _disposed;

                public void Dispose()
                {
                    if (_disposed)
                        return;

                    _disposed = true;
                    source.Unsubscribe(observer);
                }
            }
        }

        public static ICollectionObservable<T> AsObservable<T>(this ObservableSet<T> set)
        {
            return new StatefulSetObservable<T>(set);
        }

        private class StatefulSetObservable<T> : ICollectionObservable<T>
        {
            private ObservableSet<T> _source;
            private CollectionEventArgs<T> _args;
            private List<IObserver<ICollectionEventArgs<T>>> _observers = new List<IObserver<ICollectionEventArgs<T>>>();

            public StatefulSetObservable(ObservableSet<T> source)
            {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<ICollectionEventArgs<T>> observer)
            {
                _observers.Add(observer);

                if (_observers.Count == 1)
                    _source.context.RegisterObserver(HandleSourceChanged, new ObserverParameters() { scope = ObservationScope.Self }, _source);

                _args.operationType = OpType.Add;

                foreach (var element in _source)
                {
                    _args.element = element;
                    observer.OnNext(_args);
                }

                return new ObserverHandle() { observer = observer, source = this };
            }

            private void HandleSourceChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                    return;

                foreach (var change in args.changes)
                {
                    switch (change.changeType)
                    {
                        case ChangeType.Add:
                            _args.operationType = OpType.Add;
                            _args.element = (T)change.collectionElement;
                            NotifyObservers(_args);
                            break;

                        case ChangeType.Remove:
                            _args.operationType = OpType.Remove;
                            _args.element = (T)change.collectionElement;
                            NotifyObservers(_args);
                            break;

                        case ChangeType.Dispose:
                            DisposeObservers();
                            return;
                    }
                }
            }

            private void NotifyObservers(ICollectionEventArgs<T> args)
            {
                foreach (var observer in _observers)
                    observer.OnNext(args);
            }

            private void DisposeObservers()
            {
                foreach (var observer in _observers)
                    observer.OnDispose();

                _observers.Clear();
            }

            private void Unsubscribe(IObserver<ICollectionEventArgs<T>> observer)
            {
                if (_observers.Remove(observer) && _observers.Count == 0)
                    _source.context.DeregisterObserver(HandleSourceChanged);
            }

            private class ObserverHandle : IDisposable
            {
                public IObserver<ICollectionEventArgs<T>> observer;
                public StatefulSetObservable<T> source;

                private bool _disposed;

                public void Dispose()
                {
                    if (_disposed)
                        return;

                    _disposed = true;
                    source.Unsubscribe(observer);
                }
            }
        }

        public static IValueObservable<T> AsObservable<T>(this ObservablePrimitive<T> primitive)
        {
            return new StatefulPrimitiveObservable<T>(primitive);
        }

        private class StatefulPrimitiveObservable<T> : IValueObservable<T>
        {
            private ObservablePrimitive<T> _source;
            private T _previousValue;
            private ValueEventArgs<T> _args;
            private List<IObserver<IValueEventArgs<T>>> _observers = new List<IObserver<IValueEventArgs<T>>>();

            public StatefulPrimitiveObservable(ObservablePrimitive<T> source)
            {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<IValueEventArgs<T>> observer)
            {
                _observers.Add(observer);

                if (_observers.Count == 1)
                    _source.context.RegisterObserver(HandleSourceChanged, new ObserverParameters() { scope = ObservationScope.Self }, _source);

                _args.currentValue = _source.value;
                _args.previousValue = default;

                observer.OnNext(_args);

                return new ObserverHandle() { observer = observer, source = this };
            }

            private void HandleSourceChanged(NodeChangeEventArgs args)
            {
                if (args.initialize)
                    return;

                if (_source.disposed)
                {
                    foreach (var observer in _observers)
                        observer.OnDispose();

                    _observers.Clear();

                    return;
                }

                _args.previousValue = _previousValue;
                _args.currentValue = _source.value;

                foreach (var observer in _observers)
                    observer.OnNext(_args);

                _previousValue = _source.value;
            }

            private void Unsubscribe(IObserver<IValueEventArgs<T>> observer)
            {
                if (_observers.Remove(observer) && _observers.Count == 0)
                    _source.context.DeregisterObserver(HandleSourceChanged);
            }

            private class ObserverHandle : IDisposable
            {
                public IObserver<IValueEventArgs<T>> observer;
                public StatefulPrimitiveObservable<T> source;

                private bool _disposed;

                public void Dispose()
                {
                    if (_disposed)
                        return;

                    _disposed = true;
                    source.Unsubscribe(observer);
                }
            }
        }
    }
}