using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ListSelectDynamic<T, U> : IDisposable
    {
        private IDisposable _sourceStream;
        private Func<T, U> _select;
        private IListObserver<U> _receiver;
        private List<U> _selectedElements = new List<U>();
        private bool _disposed;

        public ListSelectDynamic(IListObservable<T> source, Func<T, U> select, IListObserver<U> receiver)
        {
            _receiver = receiver;
            _select = select;
            _sourceStream = source.Subscribe(new ListObserver<T>(
                onAdd: HandleAdd,
                onRemove: HandleRemove,
                onError: receiver.OnError,
                onDispose: Dispose
            ));
        }

        private void HandleAdd(uint id, int index, T value)
        {
            var selected = _select(value);
            _selectedElements.Insert(index, selected);
            _receiver.OnAdd(id, index, selected);
        }

        private void HandleRemove(uint id, int index, T _)
        {
            var selected = _selectedElements[index];
            _selectedElements.RemoveAt(index);
            _receiver.OnRemove(id, index, selected);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _receiver.OnDispose();
        }
    }
}