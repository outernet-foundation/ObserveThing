using System;
using System.Collections;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CollectionObservable<T> : ICollectionObservable<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();

        private List<T> _collection = new List<T>();
        private CollectionEventArgs<T> _args = new CollectionEventArgs<T>();
        private List<Instance> _instances = new List<Instance>();
        private bool _disposed;
        private IDisposable _fromSubscription;

        public void Add(T element)
        {
            _collection.Add(element);
            _args.element = element;
            _args.operationType = OpType.Add;
            foreach (var instance in _instances)
                instance.OnNext(_args);
        }

        public bool Remove(T element)
        {
            if (!_collection.Remove(element))
                return false;

            _collection.Remove(element);
            _args.element = element;
            _args.operationType = OpType.Remove;
            foreach (var instance in _instances)
                instance.OnNext(_args);

            return true;
        }

        public void Clear()
        {
            foreach (var element in _collection.ToArray())
                Remove(element);
        }

        public void From(IEnumerable<T> source)
        {
            _fromSubscription?.Dispose();
            Clear();

            foreach (var element in source)
                Add(element);
        }

        public void From(ICollectionObservable<T> source)
        {
            _fromSubscription?.Dispose();
            _fromSubscription = source.Subscribe(
                x =>
                {
                    if (x.operationType == OpType.Add)
                        Add(x.element);
                    else if (x.operationType == OpType.Remove)
                        Remove(x.element);
                }
            );
        }

        public IDisposable Subscribe(IObserver<ICollectionEventArgs<T>> observer)
        {
            var instance = new Instance(observer, x =>
            {
                if (!_disposed)
                    _instances.Remove(x);
            });

            _instances.Add(instance);

            foreach (var kvp in _collection)
            {
                _args.element = kvp;
                _args.operationType = OpType.Add;
                instance.OnNext(_args);
            }

            return instance;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var instance in _instances)
                instance.Dispose();

            _instances.Clear();
            _fromSubscription?.Dispose();
        }

        private class Instance : IDisposable
        {
            private IObserver<ICollectionEventArgs<T>> _observer;
            private Action<Instance> _onDispose;

            public Instance(IObserver<ICollectionEventArgs<T>> observer, Action<Instance> onDispose)
            {
                _observer = observer;
                _onDispose = onDispose;
            }

            public void OnNext(ICollectionEventArgs<T> args)
            {
                _observer?.OnNext(args);
            }

            public void OnError(Exception error)
            {
                _observer?.OnError(error);
            }

            public void Dispose()
            {
                if (_observer == null)
                    return;

                _observer.OnDispose();
                _observer = null;

                _onDispose(this);
            }
        }
    }
}