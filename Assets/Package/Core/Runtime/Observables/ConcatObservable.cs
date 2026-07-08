using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ConcatObservable<T> : IDisposable
    {
        private ICollectionOperand<T> _operand;
        private CollectionIdProvider _idProvider;
        private Dictionary<uint, uint> _source1IdMap = new Dictionary<uint, uint>();
        private Dictionary<uint, uint> _source2IdMap = new Dictionary<uint, uint>();

        private IDisposable _subscription;

        public ConcatObservable(ICollectionObservable<T> source1, ICollectionObservable<T> source2, ICollectionOperand<T> operand)
        {
            _idProvider = new CollectionIdProvider(x => !_source1IdMap.ContainsValue(x) && !_source2IdMap.ContainsValue(x));
            _operand = operand;
            _subscription = new ComposedDisposable(

                source1.SubscribeWithId(
                    onAdd: Source1HandleAdd,
                    onRemove: Source1HandleRemove,
                    onError: _operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source2.SubscribeWithId(
                    onAdd: Source2HandleAdd,
                    onRemove: Source2HandleRemove,
                    onError: _operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        private void Source1HandleAdd(uint id, T value)
        {
            var newId = _idProvider.GetUnusedId();
            _source1IdMap.Add(id, newId);
            _operand.Add(newId, value);
        }

        private void Source1HandleRemove(uint id, T value)
        {
            var newId = _source1IdMap[id];
            _source1IdMap.Remove(id);
            _operand.Remove(newId);
        }

        private void Source2HandleAdd(uint id, T value)
        {
            var newId = _idProvider.GetUnusedId();
            _source2IdMap.Add(id, newId);
            _operand.Add(newId, value);
        }

        private void Source2HandleRemove(uint id, T value)
        {
            var newId = _source2IdMap[id];
            _source2IdMap.Remove(id);
            _operand.Remove(newId);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
            _operand.OnDisposed();
        }
    }
}