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

        public IDisposable Subscribe(IObserver<IObservableEventArgs> observer)
            => new Instance(observables, observer);

        private class Instance : IDisposable
        {
            private IObserver<IObservableEventArgs> _observer;
            private List<IObservable> _activeObservers = new List<IObservable>();
            private IDisposable _streams;
            private bool _disposed;

            public Instance(IObservable[] observables, IObserver<IObservableEventArgs> observer)
            {
                _observer = observer;
                _activeObservers.AddRange(observables);
                _streams = new ComposedDisposable(
                    observables.Select(x => x.Subscribe(
                        HandleObservableChanged,
                        HandleObservableError,
                        () => HandleObservableDisposed(x)
                    )).ToArray()
                );
            }

            private void HandleObservableChanged(IObservableEventArgs args)
            {
                _observer.OnNext(args);
            }

            public void HandleObservableError(Exception exception)
            {
                _observer.OnError(exception);
            }

            public void HandleObservableDisposed(IObservable observable)
            {
                _activeObservers.Remove(observable);
                if (_activeObservers.Count == 0)
                    Dispose();
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _streams.Dispose();
                _observer.OnDispose();
            }
        }
    }
}