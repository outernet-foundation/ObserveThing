using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ConcatObservable<T> : ObservableCollectionBase<T>
    {
        private ICollectionObservable<T> _source1;
        private ICollectionObservable<T> _source2;

        private CollectionIdProvider _idProvider;
        private Dictionary<uint, uint> _source1IdMap = new Dictionary<uint, uint>();
        private Dictionary<uint, uint> _source2IdMap = new Dictionary<uint, uint>();

        private IDisposable _subscription;
        private bool _active;

        public ConcatObservable(ICollectionObservable<T> source1, ICollectionObservable<T> source2) : base(source1.context)
        {
            _idProvider = new CollectionIdProvider(x => !_source1IdMap.ContainsValue(x) && !_source2IdMap.ContainsValue(x));
            _source1 = source1;
            _source2 = source2;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscription = new ComposedDisposable(

                _source1.SubscribeWithId(
                    onAdd: Source1HandleAdd,
                    onRemove: Source1HandleRemove,
                    onError: OnError,
                    onDispose: HandleSourceDisposed,
                    immediate: true
                ),

                _source2.SubscribeWithId(
                    onAdd: Source2HandleAdd,
                    onRemove: Source2HandleRemove,
                    onError: OnError,
                    onDispose: HandleSourceDisposed,
                    immediate: true
                )

            );
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            _subscription?.Dispose();
            _subscription = null;

            ClearInternal();
            _idProvider.Reset();
            _source1IdMap.Clear();
            _source2IdMap.Clear();
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        private void Source1HandleAdd(uint id, T value)
        {
            var newId = _idProvider.GetUnusedId();
            _source1IdMap.Add(id, newId);
            AddInternal(newId, value);
        }

        private void Source1HandleRemove(uint id, T value)
        {
            var newId = _source1IdMap[id];
            _source1IdMap.Remove(id);
            RemoveInternal(newId);
        }

        private void Source2HandleAdd(uint id, T value)
        {
            var newId = _idProvider.GetUnusedId();
            _source2IdMap.Add(id, newId);
            AddInternal(newId, value);
        }

        private void Source2HandleRemove(uint id, T value)
        {
            var newId = _source2IdMap[id];
            _source2IdMap.Remove(id);
            RemoveInternal(newId);
        }

        protected override void DisposeInternal()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}