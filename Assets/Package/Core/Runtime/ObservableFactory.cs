using System;

namespace ObserveThing
{
    public class FactoryObservable : IObservable
    {
        private Func<IObserver, IDisposable> _subscribe;

        public FactoryObservable(Func<IObserver, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IObserver observer)
            => _subscribe(observer);
    }
}