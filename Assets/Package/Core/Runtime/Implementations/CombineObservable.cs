using System;
using System.Collections.Generic;
using System.Linq;

namespace ObserveThing
{
    public class CombineObservable : IPendingObserver, IDisposable
    {
        public uint priority { get; }
        public bool immediate => _receiver.immediate;
        public bool pending;
        public bool disposed { get; private set; }

        private List<IOperation> _pendingOperations;
        private List<IOperation> _pendingOperations1 = new List<IOperation>();
        private List<IOperation> _pendingOperations2 = new List<IOperation>();

        public ObservationContext context { get; }

        private IObserver _receiver;
        private List<IObservable> _observables = new List<IObservable>();
        private IDisposable _streams;

        public CombineObservable(IObserver receiver, params IObservable[] observables) : this(default, receiver, observables) { }
        public CombineObservable(ObservationContext context, IObserver receiver, params IObservable[] observables)
        {
            this.context = context ?? Settings.DefaultObservationContext;
            priority = context.AllocateObserverPriority();

            SwitchPendingOperationsList();

            _receiver = receiver;
            _observables.AddRange(observables);
            _streams = new ComposedDisposable(_observables.Select(x => x.Subscribe(new Observer(HandleSourceChanged, onDispose: () => HandleSourceDisposed(x), immediate: true))).ToArray());

            receiver.OnOperation(null);
        }

        private void HandleSourceDisposed(IObservable source)
        {
            if (disposed)
                return;

            _observables.Remove(source);

            if (_observables.Count == 0)
                Dispose();
        }

        private void HandleSourceChanged(IReadOnlyList<IOperation> ops)
        {
            if (ops == null)
                return;

            foreach (var op in ops)
                EnqueuePendingOperation(op.Clone());
        }

        private void SwitchPendingOperationsList()
        {
            if (_pendingOperations == _pendingOperations1)
            {
                _pendingOperations = _pendingOperations2;
            }
            else
            {
                _pendingOperations = _pendingOperations1;
            }
        }

        protected void EnqueuePendingOperation(IOperation operation)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            _pendingOperations.Add(operation);

            if (!pending)
                context.RegisterPendingObserver(this);

            context.NotifyPendingObserversIfNecessary();
        }

        public void SendNext()
        {
            if (_pendingOperations.Count == 0)
                return;

            var ops = _pendingOperations;
            SwitchPendingOperationsList();
            pending = false;

            try
            {
                _receiver.OnOperation(ops);
            }
            catch (Exception exc)
            {
                _receiver.OnError(exc);
            }

            ops.Clear();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            _streams?.Dispose();

            context.DeallocateObserverPriority(priority);

            _receiver.OnDispose();
        }
    }
}