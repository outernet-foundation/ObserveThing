
using System.Collections.Generic;

namespace ObserveThing
{
    public class Observable<T> : ObservableBase<Observer<T>, T> where T : IOperation
    {
        private IInitializationOperationsProvider<T> _initializationOperationsProvider;

        public Observable(ObservationContext context, IInitializationOperationsProvider<T> initializationOperationsProvider) : base(context)
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

        protected override void SendOperation(Observer<T> observer, T operation)
            => observer.OnNext(operation);

        public void EnqueueOperation(T operation)
            => EnqueuePendingOperation(operation);
    }
}