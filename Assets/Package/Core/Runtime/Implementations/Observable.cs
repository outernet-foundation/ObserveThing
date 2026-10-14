
using System.Collections.Generic;

namespace ObserveThing
{
    public class OpStreamObservable<T> : ObservableBase<IOpStreamObserver<T>, T>
    {
        private IInitializationOperationsProvider<T> _initializationOperationsProvider;

        public OpStreamObservable(ObservationContext context, IInitializationOperationsProvider<T> initializationOperationsProvider) : base(context)
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

        protected override void SendOperation(IOpStreamObserver<T> observer, T operation)
            => observer.OnNext(operation);

        public void EnqueueOperation(T operation)
            => EnqueuePendingOperation(operation);
    }
}