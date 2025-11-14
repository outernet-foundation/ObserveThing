using UnityEngine;
using System;

namespace ObserveThing
{
    public class PreserveOrderObservable<T, U> : IListObservable<U>
    {
        private IListObservable<T> _source;
        private Func<ICollectionObservable<T>, ICollectionObservable<U>> _generateTransformedCollection;

        public PreserveOrderObservable(IListObservable<T> source, Func<ICollectionObservable<T>, ICollectionObservable<U>> generateTransformedCollection)
        {
            _source = source;
            _generateTransformedCollection = generateTransformedCollection;
        }

        public IDisposable Subscribe(IObserver<IListEventArgs<U>> observer)
        {
            return new Instance(this, _source, _generateTransformedCollection, observer);
        }

        private class Instance : IDisposable
        {
            private IObserver<IListEventArgs<U>> _observer;
            private ListEventArgs<U> _args = new ListEventArgs<U>();
            private bool _disposed = false;

            private IDisposable _listStream;

            private CollectionObservable<T> _internalCollection = new CollectionObservable<T>();
            private IDisposable _transformedCollectionStream;

            public Instance(IObservable source, IListObservable<T> list, Func<ICollectionObservable<T>, ICollectionObservable<U>> generateTransformedCollection, IObserver<IListEventArgs<U>> observer)
            {
                _observer = observer;
                _args.source = source;

                _transformedCollectionStream = generateTransformedCollection(_internalCollection).Subscribe(
                    HandleTransformedCollectionChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );

                _listStream = list.Subscribe(
                    HandleListChanged,
                    HandleSourceError,
                    HandleSourceDisposed
                );
            }

            private void HandleListChanged(IListEventArgs<T> args)
            {
                Debug.Log("EP: Receiving args as " + args.operationType);

                _args.index = args.index;
                _args.operationType = args.operationType;

                if (args.operationType == OpType.Add)
                {
                    _internalCollection.Add(args.element);
                }
                else if (args.operationType == OpType.Remove)
                {
                    _internalCollection.Remove(args.element);
                }
            }

            private void HandleTransformedCollectionChanged(ICollectionEventArgs<U> args)
            {
                _args.element = args.element;
                Debug.Log("EP: Sending args as " + _args.operationType);
                _observer.OnNext(_args);
            }

            private void HandleSourceError(Exception error)
            {
                _observer.OnError(error);
            }

            private void HandleSourceDisposed()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                _transformedCollectionStream.Dispose();
                _listStream.Dispose();
                _observer.OnDispose();
            }
        }
    }
}