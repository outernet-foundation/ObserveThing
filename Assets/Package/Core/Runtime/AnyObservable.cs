using System;
using System.Linq;

namespace ObserveThing
{
    public class AnyObservable : IDisposable
    {
        private IDisposable _subscription;
        private IObserver _receiver;
        private bool _updated;
        private bool _disposed;

        public AnyObservable(IObservable[] observables, IObserver receiver)
        {
            _receiver = receiver;
            _subscription = new ComposedDisposable(observables.Select(x => x.Subscribe(
                onChange: OnChange,
                onError: receiver.OnError,
                immediate: receiver.immediate
            )).ToArray());

            if (!_updated)
                receiver.OnChange();
        }

        private void OnChange()
        {
            _updated = true;
            _receiver.OnChange();
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
