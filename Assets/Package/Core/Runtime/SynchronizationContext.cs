using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public abstract class SynchronizationContext
    {
        public static SynchronizationContext Default = new DefaultSynchronizationContext();
        public abstract void EnqueueAction(Action action);
    }

    public class DefaultSynchronizationContext : SynchronizationContext
    {
        private Queue<Action> _actionQueue = new Queue<Action>();
        private bool _executingActions = false;

        public override void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);

            if (_executingActions)
                return;

            _executingActions = true;

            while (_actionQueue.TryDequeue(out var nextAction))
                nextAction?.Invoke();

            _executingActions = false;
        }
    }
}