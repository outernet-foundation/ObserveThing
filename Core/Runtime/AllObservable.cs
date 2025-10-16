using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class AllObservable : IObservable
    {
        public IObservable[] observables;

        public AllObservable(params IObservable[] observables)
        {
            this.observables = observables;
        }

        public IDisposable Subscribe(IObserver<ObservableEventArgs> observer)
            => new Instance(observables, observer);

        private class Instance : IDisposable
        {
            private IObserver<ObservableEventArgs> _observer;
            private List<IObservable> _activeObservers = new List<IObservable>();
            private IDisposable _streams;
            private bool _disposed;

            public Instance(IObservable[] observables, IObserver<ObservableEventArgs> observer)
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

            private void HandleObservableChanged(ObservableEventArgs args)
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