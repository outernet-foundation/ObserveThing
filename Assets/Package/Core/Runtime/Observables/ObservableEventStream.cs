
using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableEventStream<T> : Observable<IEventStreamObserver<T>, T>, IEventStreamObservable<T>
    {
        private IInitializationOperationsProvider<T> _initializationOperationsProvider;

        public ObservableEventStream(ObservationContext context, IInitializationOperationsProvider<T> initializationOperationsProvider) : base(context)
        {
            _initializationOperationsProvider = initializationOperationsProvider;
        }

        protected override IEnumerable<T> GetInitializationOperations()
        {
            if (_initializationOperationsProvider == null)
                yield break;

            foreach (var op in _initializationOperationsProvider.GetInitializationOperations())
                yield return op;
        }

        protected override void SendOperation(IEventStreamObserver<T> observer, T operation)
            => observer.OnNext(operation);

        public void EnqueueOperation(T operation)
            => EnqueuePendingOperation(operation);

        public IDisposable Subscribe(IEventStreamObserver observer, bool immediate = false, uint? priority = null)
            => Subscribe(new EventStreamObserver<T>(
                onNext: x => observer.OnNext(x),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}