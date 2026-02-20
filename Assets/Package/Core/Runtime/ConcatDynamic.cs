using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ConcatDynamic<T> : IDisposable
    {
        private IDisposable _source1Stream;
        private IDisposable _source2Stream;
        private ICollectionObserver<T> _receiver;
        private bool _disposed;

        private CollectionIdProvider _idProvider;
        private Dictionary<uint, uint> _source1IdMap = new Dictionary<uint, uint>();
        private Dictionary<uint, uint> _source2IdMap = new Dictionary<uint, uint>();

        public ConcatDynamic(ICollectionObservable<T> source1, ICollectionObservable<T> source2, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;
            _idProvider = new CollectionIdProvider(x => !_source1IdMap.ContainsValue(x) && !_source2IdMap.ContainsValue(x));

            _source1Stream = source1.SubscribeWithId(
                onAdd: Source1HandleAdd,
                onRemove: Source1HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );

            _source2Stream = source2.SubscribeWithId(
                onAdd: Source2HandleAdd,
                onRemove: Source2HandleRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
        }

        private void Source1HandleAdd(uint id, T value)
        {
            var newId = _idProvider.GetUnusedId();
            _source1IdMap.Add(id, newId);
            _receiver.OnAdd(newId, value);
        }

        private void Source1HandleRemove(uint id, T value)
        {
            var newId = _source1IdMap[id];
            _source1IdMap.Remove(id);
            _receiver.OnRemove(newId, value);
        }

        private void Source2HandleAdd(uint id, T value)
        {
            var newId = _idProvider.GetUnusedId();
            _source2IdMap.Add(id, newId);
            _receiver.OnAdd(newId, value);
        }

        private void Source2HandleRemove(uint id, T value)
        {
            var newId = _source2IdMap[id];
            _source2IdMap.Remove(id);
            _receiver.OnRemove(newId, value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _source1Stream.Dispose();
            _source2Stream.Dispose();

            _receiver.OnDispose();
        }
    }
}