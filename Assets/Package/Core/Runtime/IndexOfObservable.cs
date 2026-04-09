using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class IndexOfObservable<T> : IDisposable
    {
        private IDisposable _valueStream;
        private IDisposable _sourceStream;
        private IValueObserver<int> _receiver;
        private T _latest = default;
        private List<T> _list = new List<T>();
        private int _index = -1;
        private bool _disposed;

        public IndexOfObservable(ICollectionObservable<T> source, IValueObservable<T> value, IValueObserver<int> receiver)
        {
            _receiver = receiver;
            _valueStream = value.Subscribe(
                onNext: HandleNext,
                onError: receiver.OnError,
                onDispose: Dispose,
                immediate: receiver.immediate
            );

            _sourceStream = source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: receiver.OnError,
                onDispose: Dispose,
                immediate: receiver.immediate
            );

            if (_index == -1)
                _receiver.OnNext(-1);
        }

        private void HandleAdd(T element)
        {
            _list.Add(element);
            UpdateIndexIfNecessary();
        }

        private void HandleRemove(T element)
        {
            _list.Remove(element);
            UpdateIndexIfNecessary();
        }

        private void HandleNext(T value)
        {
            _latest = value;
            UpdateIndexIfNecessary();
        }

        private void UpdateIndexIfNecessary()
        {
            var newIndex = -1;

            for (int i = 0; i < _list.Count; i++)
            {
                if (Equals(_latest, _list[i]))
                {
                    newIndex = i;
                    break;
                }
            }

            if (_index == newIndex)
                return;

            _index = newIndex;

            _receiver.OnNext(_index);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();
            _valueStream.Dispose();

            _receiver.OnDispose();
        }
    }
}