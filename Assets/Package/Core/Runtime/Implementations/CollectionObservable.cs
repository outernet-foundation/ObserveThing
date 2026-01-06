using System;
using System.Collections;
using System.Collections.Generic;

namespace ObserveThing
{
    public class CollectionObservable<T> : ICollectionObservable<T>, IEnumerable<T>
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();

        public int count => _collection.Count;

        private List<T> _collection = new List<T>();
        private List<Instance> _instances = new List<Instance>();
        private List<Instance> _disposedInstances = new List<Instance>();
        private bool _executingOnNext = false;
        private bool _disposed;

        public CollectionObservable(params T[] source) : this((IEnumerable<T>)source) { }

        public CollectionObservable(IEnumerable<T> source) : this()
        {
            _collection.AddRange(source);
        }

        public CollectionObservable() { }

        private void SafeOnNext(IObservable source, T element, OpType opType)
        {
            _executingOnNext = true;

            int count = _instances.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _instances[i];
                if (instance.disposed)
                    continue;

                instance.OnNext(source, element, opType);
            }

            foreach (var disposedInstance in _disposedInstances)
                _instances.Remove(disposedInstance);

            _executingOnNext = false;
        }

        public void Add(T element)
        {
            _collection.Add(element);
            SafeOnNext(this, element, OpType.Add);
        }

        public bool Remove(T element)
        {
            if (!_collection.Remove(element))
                return false;

            SafeOnNext(this, element, OpType.Remove);

            return true;
        }

        public void Clear()
        {
            foreach (var element in _collection.ToArray())
            {
                _collection.Remove(element);
                SafeOnNext(this, element, OpType.Remove);
            }
        }

        public bool Contains(T element)
            => _collection.Contains(element);

        public IDisposable Subscribe(IObserver<ICollectionEventArgs<T>> observer)
        {
            var instance = new Instance(observer, x =>
            {
                if (_disposed)
                    return;

                if (_executingOnNext)
                {
                    _disposedInstances.Add(x);
                    return;
                }

                _instances.Remove(x);
            });

            _instances.Add(instance);

            foreach (var element in _collection)
                instance.OnNext(this, element, OpType.Add);

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
        }

        private class Instance : IDisposable
        {
            public bool disposed { get; private set; }

            private IObserver<ICollectionEventArgs<T>> _observer;
            private Action<Instance> _onDispose;
            private CollectionEventArgs<T> _args = new CollectionEventArgs<T>();

            public Instance(IObserver<ICollectionEventArgs<T>> observer, Action<Instance> onDispose)
            {
                _observer = observer;
                _onDispose = onDispose;
            }

            public void OnNext(IObservable source, T element, OpType opType)
            {
                _args.source = source;
                _args.element = element;
                _args.operationType = opType;
                _observer?.OnNext(_args);
            }

            public void OnError(Exception error)
            {
                _observer?.OnError(error);
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                _observer.OnDispose();
                _observer = null;

                _onDispose(this);
            }
        }
    }
}