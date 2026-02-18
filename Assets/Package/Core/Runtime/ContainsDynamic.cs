using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ContainsDynamic<T> : IDisposable
    {
        private IDisposable _sourceStream;
        private IDisposable _valueStream;
        private IValueObserver<bool> _receiver;
        private List<T> _list = new List<T>();
        private T _latest = default;
        private bool _present = false;
        private bool _disposed;

        public ContainsDynamic(ICollectionObservable<T> source, IValueObservable<T> value, IValueObserver<bool> receiver)
        {
            _sourceStream = source.Subscribe(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );

            _valueStream = value.Subscribe(
                onNext: HandleNext,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void HandleAdd(T element)
        {
            _list.Add(element);

            if (_present)
                return;

            if (Equals(element, _latest))
            {
                _present = true;
                _receiver.OnNext(true);
            }
        }

        private void HandleRemove(T element)
        {
            _list.Remove(element);

            if (!_present)
                return;

            if (Equals(element, _latest) && !_list.Contains(element))
            {
                _present = false;
                _receiver.OnNext(false);
            }
        }

        private void HandleNext(T value)
        {
            _latest = value;
            bool wasPresent = _present;
            _present = _list.Contains(value);

            if (_present == wasPresent)
                return;

            _receiver.OnNext(_present);
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