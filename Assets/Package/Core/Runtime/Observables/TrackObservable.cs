using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class TrackObservable<TKey, TValue> : ObservableValueBase<(bool present, TValue value)>
    {
        private IDictionaryObservable<TKey, TValue> _source;
        private IValueObservable<TKey> _keySource;
        private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        private TKey _key = default;
        private bool _present = false;

        private IDisposable _subscriptions;
        private bool _active;

        public TrackObservable(IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key) : base(source.context)
        {
            _source = source;
            _keySource = key;
        }

        protected override void OnFirstObserverAdded()
        {
            _active = true;
            _subscriptions = new ComposedDisposable(

                _source.Subscribe(
                    onAdd: kvp =>
                    {
                        _dict.Add(kvp.Key, kvp.Value);
                        if (Equals(kvp.Key, _key))
                        {
                            _present = true;
                            SetValueInternal(new(_present, kvp.Value));
                        }
                    },
                    onRemove: kvp =>
                    {
                        if (_dict.Remove(kvp.Key) && Equals(kvp.Key, _key))
                        {
                            _present = false;
                            SetValueInternal(new(_present, default));
                        }
                    },
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _keySource.Subscribe(
                    onNext: key =>
                    {
                        _key = key;

                        if (_dict.TryGetValue(_key, out var value))
                        {
                            _present = true;
                            SetValueInternal(new(_present, value));
                        }
                        else
                        {
                            _present = false;
                            SetValueInternal(new(_present, default));
                        }
                    },
                    onError: OnError,
                    onDispose: HandleSourceDisposed,
                    immediate: true
                )

            );
        }

        protected override void OnLastObserverRemoved()
        {
            _active = false;
            _subscriptions?.Dispose();
            _subscriptions = null;

            _key = default;
            _dict.Clear();

            SetValueInternal(default);
        }

        private void HandleSourceDisposed()
        {
            if (!_active)
                return;

            Dispose();
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}
