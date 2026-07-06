using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IPendingObserver
    {
        uint priority { get; }
        bool immediate { get; }
        bool disposed { get; }
        void SendNext();
    }

    public class ObservationContext
    {
        private PriorityQueue<IPendingObserver, ObserverOrder> _pendingImmediateObservers = new PriorityQueue<IPendingObserver, ObserverOrder>(new ObserverOrderComparer());
        private PriorityQueue<IPendingObserver, ObserverOrder> _pendingObservers = new PriorityQueue<IPendingObserver, ObserverOrder>(new ObserverOrderComparer());

        private HashSet<uint> _allocatedPriorties = new HashSet<uint>();
        private CollectionIdProvider _idProvider;

        private bool _notifyingImmediateObservers = false;
        private bool _notifyingObservers = false;
        private bool _executingBatch = false;
        private int _registrationOrder = 0;

        private struct ObserverOrder
        {
            public uint priority;
            public int registrationOrder;
        }

        private class ObserverOrderComparer : IComparer<ObserverOrder>
        {
            public int Compare(ObserverOrder x, ObserverOrder y)
            {
                if (x.priority != y.priority)
                    return x.priority.CompareTo(y.priority);

                return x.registrationOrder.CompareTo(y.registrationOrder);
            }
        }

        public ObservationContext()
        {
            _idProvider = new CollectionIdProvider(x => _allocatedPriorties.Contains(x));
        }

        public void ExecuteBatchOperation(Action batchOperation)
        {
            bool wasExecutingBatch = _executingBatch;
            _executingBatch = true;
            batchOperation.Invoke();
            _executingBatch = wasExecutingBatch;

            NotifyPendingObserversIfNecessary();
        }

        private void DrainPendingObserverQueue()
        {
            while (_pendingObservers.TryDequeue(out var observer, out var _))
            {
                if (observer.disposed)
                    continue;

                observer.SendNext();
            }
        }

        private void DrainPendingImmediateObserverQueue()
        {
            while (_pendingImmediateObservers.TryDequeue(out var observer, out var _))
            {
                if (observer.disposed)
                    continue;

                observer.SendNext();
            }
        }

        public uint AllocateObserverPriority()
        {
            var priority = _idProvider.GetUnusedId();
            _allocatedPriorties.Add(priority);
            return priority;
        }

        public void DeallocateObserverPriority(uint priority)
        {
            _allocatedPriorties.Remove(priority);
        }

        public void RegisterPendingObserver(IPendingObserver observer)
        {
            if (observer.immediate)
            {
                _pendingImmediateObservers.Enqueue(observer, new() { priority = observer.priority, registrationOrder = _registrationOrder });
            }
            else
            {
                _pendingObservers.Enqueue(observer, new() { priority = observer.priority, registrationOrder = _registrationOrder });
            }

            _registrationOrder++;
        }

        public void NotifyPendingObserversIfNecessary()
        {
            if (_notifyingImmediateObservers)
                return;

            _notifyingImmediateObservers = true;
            DrainPendingImmediateObserverQueue(); // immediate notifications should get sent even if we're in a batch
            _notifyingImmediateObservers = false;

            if (_notifyingObservers || _executingBatch)
                return;

            _notifyingObservers = true;
            DrainPendingObserverQueue();
            _notifyingObservers = false;

            _registrationOrder = 0;
        }
    }
}