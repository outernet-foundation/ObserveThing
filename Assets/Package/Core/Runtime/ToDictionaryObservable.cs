using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ObserveThing
{
    public class ToDictionaryObservable<TSource, TKey, TValue> : IDictionaryObservable<TKey, TValue>
    {
        private ICollectionObservable<TSource> _source;
        private Func<TSource, IValueObservable<TKey>> _selectKey;
        private Func<TSource, IValueObservable<TValue>> _selectValue;

        public ToDictionaryObservable(ICollectionObservable<TSource> source, Func<TSource, IValueObservable<TKey>> selectKey, Func<TSource, IValueObservable<TValue>> selectValue)
        {
            _source = source;
            _selectKey = selectKey;
            _selectValue = selectValue;
        }

        public IDisposable Subscribe(IObserver<IDictionaryEventArgs<TKey, TValue>> observer)
            => new Instance(this, observer, _source, _selectKey, _selectValue);

        private class Instance : IDisposable
        {
            private IObserver<IDictionaryEventArgs<TKey, TValue>> _observer;
            private DictionaryEventArgs<TKey, TValue> _args = new DictionaryEventArgs<TKey, TValue>();
            private IDisposable _collectionStream;
            private Func<TSource, IValueObservable<TKey>> _selectKey;
            private Func<TSource, IValueObservable<TValue>> _selectValue;
            private Dictionary<TSource, ElementData> _elementData = new Dictionary<TSource, ElementData>();
            private HashSet<TKey> _keys = new HashSet<TKey>();
            private bool _disposed;

            private class ElementData
            {
                public TSource source;
                public TKey currentKey;
                public TValue currentValue;
                public IDisposable keyStream;
                public IDisposable valueStream;
                public int count;
                public bool initialized;
                public bool disposed;

                public void Dispose()
                {
                    if (disposed)
                        return;

                    disposed = true;
                    keyStream?.Dispose();
                    valueStream?.Dispose();
                }
            }

            public Instance(
                IObservable source,
                IObserver<IDictionaryEventArgs<TKey, TValue>> observer,
                ICollectionObservable<TSource> collection,
                Func<TSource, IValueObservable<TKey>> selectKey,
                Func<TSource, IValueObservable<TValue>> selectValue
            )
            {
                _observer = observer;
                _args.source = source;
                _selectKey = selectKey;
                _selectValue = selectValue;

                _collectionStream = collection.Subscribe(
                    HandleSourceUpdated,
                    HandleSourceError,
                    HandleSourceDisposed,
                    "dict observer"
                );
            }

            private void HandleSourceUpdated(ICollectionEventArgs<TSource> args)
            {
                if (args.operationType == OpType.Add)
                {
                    if (_elementData.ContainsKey(args.element))
                        throw new Exception("ToDictionaryDynamic cannot handle duplicate elements. Consider using DistinctDynamic first.");

                    var elementData = new ElementData() { source = args.element };
                    _elementData.Add(args.element, elementData);

                    elementData.keyStream = _selectKey(args.element).Subscribe(
                        keyArgs => HandleElementKeyChanged(elementData, keyArgs),
                        HandleSourceError,
                        () => HandleElementSourceDisposed(elementData),
                        "key observer"
                    );

                    elementData.valueStream = _selectValue(args.element).Subscribe(
                        valueArgs => HandleElementValueChanged(elementData, valueArgs),
                        HandleSourceError,
                        () => HandleElementSourceDisposed(elementData),
                        "value observer"
                    );

                    elementData.initialized = true;

                    _args.element = new KeyValuePair<TKey, TValue>(elementData.currentKey, elementData.currentValue);
                    _args.operationType = OpType.Add;
                    _observer.OnNext(_args);
                }
                else if (args.operationType == OpType.Remove)
                {
                    var elementData = _elementData[args.element];
                    elementData.Dispose();
                    _elementData.Remove(args.element);
                    _keys.Remove(elementData.currentKey);
                    _args.element = new KeyValuePair<TKey, TValue>(elementData.currentKey, elementData.currentValue);
                    _args.operationType = OpType.Remove;
                    _observer.OnNext(_args);
                }
            }

            private void HandleElementSourceDisposed(ElementData elementData)
            {
                if (elementData.disposed)
                    return;

                HandleSourceError(new Exception("Element source disposed unexpectedly."));
            }

            private void HandleElementKeyChanged(ElementData elementData, IValueEventArgs<TKey> keyArgs)
            {
                if (_keys.Contains(keyArgs.currentValue))
                    throw new ArgumentException($"An item with the same key has already been added. Key: {keyArgs.currentValue}");

                _keys.Remove(keyArgs.previousValue);
                _keys.Add(keyArgs.currentValue);

                if (!elementData.initialized)
                {
                    elementData.currentKey = keyArgs.currentValue;
                    return;
                }

                _args.element = new KeyValuePair<TKey, TValue>(elementData.currentKey, elementData.currentValue);
                _args.operationType = OpType.Remove;

                elementData.currentKey = keyArgs.currentValue;

                _observer.OnNext(_args);

                _args.element = new KeyValuePair<TKey, TValue>(elementData.currentKey, elementData.currentValue);
                _args.operationType = OpType.Add;

                _observer.OnNext(_args);
            }

            private void HandleElementValueChanged(ElementData elementData, IValueEventArgs<TValue> valueArgs)
            {
                if (!elementData.initialized)
                {
                    elementData.currentValue = valueArgs.currentValue;
                    return;
                }

                _args.element = new KeyValuePair<TKey, TValue>(elementData.currentKey, elementData.currentValue);
                _args.operationType = OpType.Remove;

                elementData.currentValue = valueArgs.currentValue;

                _observer.OnNext(_args);

                _args.element = new KeyValuePair<TKey, TValue>(elementData.currentKey, elementData.currentValue);
                _args.operationType = OpType.Add;

                _observer.OnNext(_args);
            }

            private void HandleSourceError(Exception exception)
            {
                _observer.OnError(exception);
            }

            private void HandleSourceDisposed()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _collectionStream.Dispose();

                foreach (var element in _elementData.Values)
                    element.Dispose();

                _observer.OnDispose();
            }
        }
    }
}