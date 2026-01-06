using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class DictionaryObservable<TKey, TValue> : IDictionaryObservable<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        public int count => _dictionary.Count;

        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        private List<Instance> _instances = new List<Instance>();
        private List<Instance> _disposedInstances = new List<Instance>();
        private bool _executingOnNext;
        private bool _disposed;

        public DictionaryObservable() { }

        private void SafeOnNext(IObservable source, TKey key, TValue value, OpType opType)
        {
            _executingOnNext = true;

            int count = _instances.Count;
            for (int i = 0; i < count; i++)
            {
                var instance = _instances[i];
                if (instance.disposed)
                    continue;

                instance.OnNext(source, key, value, opType);
            }

            foreach (var disposedInstance in _disposedInstances)
                _instances.Remove(disposedInstance);

            _executingOnNext = false;
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            SafeOnNext(this, key, value, OpType.Add);
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var value))
                return false;

            _dictionary.Remove(key);
            SafeOnNext(this, key, value, OpType.Remove);

            return true;
        }

        public void Clear()
        {
            foreach (var kvp in _dictionary.ToArray())
            {
                _dictionary.Remove(kvp.Key);
                SafeOnNext(this, kvp.Key, kvp.Value, OpType.Remove);
            }
        }

        public bool ContainsKey(TKey key)
            => _dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value)
            => _dictionary.ContainsValue(value);

        public IDisposable Subscribe(IObserver<IDictionaryEventArgs<TKey, TValue>> observer)
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

            foreach (var kvp in _dictionary)
                instance.OnNext(this, kvp.Key, kvp.Value, OpType.Add);

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

            private IObserver<DictionaryEventArgs<TKey, TValue>> _observer;
            private DictionaryEventArgs<TKey, TValue> _args = new DictionaryEventArgs<TKey, TValue>();
            private Action<Instance> _onDispose;

            public Instance(IObserver<DictionaryEventArgs<TKey, TValue>> observer, Action<Instance> onDispose)
            {
                _observer = observer;
                _onDispose = onDispose;
            }

            public void OnNext(IObservable source, TKey key, TValue value, OpType opType)
            {
                _args.source = source;
                _args.element = new KeyValuePair<TKey, TValue>(key, value);
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