using System;

namespace ObserveThing
{
    public class ConcatDynamic<T> : IDisposable
    {
        private IDisposable _source1Stream;
        private IDisposable _source2Stream;
        private ICollectionObserver<T> _receiver;
        private bool _disposed;

        public ConcatDynamic(ICollectionObservable<T> source1, ICollectionObservable<T> source2, ICollectionObserver<T> receiver)
        {
            _receiver = receiver;

            _source1Stream = source1.Subscribe(
                onAdd: _receiver.OnAdd,
                onRemove: _receiver.OnRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );

            _source2Stream = source2.Subscribe(
                onAdd: _receiver.OnAdd,
                onRemove: _receiver.OnRemove,
                onError: _receiver.OnError,
                onDispose: Dispose
            );
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