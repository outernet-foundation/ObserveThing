using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class AnyObservable : IObservable
    {
        public IObservable[] observables;

        public AnyObservable(params IObservable[] observables)
        {
            this.observables = observables;
        }

        public IDisposable Subscribe(Action<IObservableEventArgs> observer)
            => new Instance(observables, observer);

        private class Instance : IDisposable
        {
            private Action<IObservableEventArgs> _observer;
            private List<IObservable> _activeObservers = new List<IObservable>();
            private IDisposable _streams;
            private bool _disposed;

            public Instance(IObservable[] observables, Action<IObservableEventArgs> observer)
            {
                _observer = observer;
                _activeObservers.AddRange(observables);
                _streams = new ComposedDisposable(
                    observables.Select(x => x.Subscribe(_observer)).ToArray()
                );
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _streams.Dispose();
                _observer(new ObservableEventArgs() { isDispose = true });
            }
        }
    }
}