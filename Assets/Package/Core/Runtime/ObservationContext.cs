using System;
using System.Collections.Generic;
using UnityEngine;

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
        private PriorityQueue<IPendingObserver, uint> _pendingImmediateObservers = new PriorityQueue<IPendingObserver, uint>();
        private PriorityQueue<IPendingObserver, uint> _pendingObservers = new PriorityQueue<IPendingObserver, uint>();

        private HashSet<uint> _allocatedPriorties = new HashSet<uint>();
        private CollectionIdProvider _idProvider;

        private bool _notifyingObservers = false;
        private bool _executingBatch = false;

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
            DrainPendingImmediateObserverQueue(); // do this first because there might not be any entries in the observer queue

            while (_pendingObservers.TryDequeue(out var observer, out var _))
            {
                if (observer.disposed)
                    continue;

                observer.SendNext();

                DrainPendingImmediateObserverQueue();
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
                _pendingImmediateObservers.Enqueue(observer, observer.priority);
            }
            else
            {
                _pendingObservers.Enqueue(observer, observer.priority);
            }
        }

        public void NotifyPendingObserversIfNecessary()
        {
            if (_notifyingObservers)
                return;

            _notifyingObservers = true;

            DrainPendingImmediateObserverQueue(); // immediate notifications should get sent even in we're in a batch

            if (!_executingBatch)
                DrainPendingObserverQueue(); // standard notifications should only go out if we're not in a batch

            _notifyingObservers = false;
        }
    }
}