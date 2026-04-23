using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public abstract class ObservableOperator<T> : IPendingObserver, IDisposable
    {
        public uint priority { get; }
        public bool immediate => _receiver.immediate;
        public bool disposed { get; private set; }

        private bool _pending;
        private List<T> _pendingOperations;
        private List<T> _pendingOperations1 = new List<T>();
        private List<T> _pendingOperations2 = new List<T>();

        private ObservationContext _context;
        private IObserver<T> _receiver;

        public ObservableOperator(ObservationContext context, IObserver<T> receiver)
        {
            _context = context ?? Settings.DefaultObservationContext;
            priority = context.AllocateObserverPriority();

            SwitchPendingOperationsList();

            _receiver = receiver;

            receiver.OnOperation(null);
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

        protected void EnqueuePendingOperation(T operation)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            _pendingOperations.Add(operation);

            if (!_pending)
            {
                _pending = true;
                _context.RegisterPendingObserver(this);
            }

            _context.NotifyPendingObserversIfNecessary();
        }

        protected virtual void DisposeInternal() { }

        public void SendNext()
        {
            if (_pendingOperations.Count == 0)
                return;

            var ops = _pendingOperations;
            SwitchPendingOperationsList();
            _pending = false;

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

            _context.DeallocateObserverPriority(priority);

            DisposeInternal();

            _receiver.OnDispose();
        }
    }
}