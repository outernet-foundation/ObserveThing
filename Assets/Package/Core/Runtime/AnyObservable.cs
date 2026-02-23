using System;
using System.Linq;

namespace ObserveThing
{
    public class AnyObservable : IDisposable
    {
        private IDisposable _subscription;
        private IObserver _receiver;

        private bool _disposed;

        public AnyObservable(IObservable[] observables, IObserver receiver)
        {
            _receiver = receiver;
            _subscription = new ComposedDisposable(observables.Select(x => x.Subscribe(
                onChange: receiver.OnChange,
                onError: receiver.OnError
            )).ToArray());
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _subscription.Dispose();
            _receiver.OnDispose();
        }
    }
}
