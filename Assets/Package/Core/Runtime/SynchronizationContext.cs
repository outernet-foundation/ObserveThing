using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObserveThing
{
    public abstract class SynchronizationContext
    {
        public static SynchronizationContext Default = new DefaultSynchronizationContext();
        public abstract void EnqueueAction(Action action);
        public abstract void PauseExecution();
        public abstract void ResumeExecution();
    }

    public class DefaultSynchronizationContext : SynchronizationContext
    {
        private const int MAX_NESTED_ENQUEUES = 100;

        private Queue<Action> _actionQueue = new Queue<Action>();
        private bool _executingActions = false;
        private bool _enqueuedDuringExecute = false;
        private int _executionPauses = 0;

        public override void PauseExecution()
        {
            _executionPauses++;
        }

        public override void ResumeExecution()
        {
            bool wasPaused = _executionPauses > 0;
            _executionPauses = Mathf.Max(0, _executionPauses - 1);
            if (_executionPauses == 0 && wasPaused && !_executingActions) // if we pause and resume during a single callback, _excecutingActions will still be true at this
                ExecutePendingActions();
        }

        public override void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);

            if (_executionPauses > 0)
                return;

            ExecutePendingActions();
        }

        public void ExecutePendingActions()
        {
            if (_executingActions)
                throw new Exception("Cannot execute pending actions while already executing.");

            _executingActions = true;

            int nestedEnqueues = 0;

            while (_actionQueue.TryDequeue(out var action))
            {
                if (nestedEnqueues >= MAX_NESTED_ENQUEUES)
                {
                    _executingActions = false;
                    throw new Exception("Max nested enqueues exceeded. Could this be an infinite loop?");
                }

                try
                {
                    action.Invoke();
                }
                catch(Exception exc)
                {
                    Debug.LogException(exc);
                }

                if (_enqueuedDuringExecute)
                {
                    nestedEnqueues++;
                    _enqueuedDuringExecute = false;
                }

                if (_executionPauses > 0)
                {
                    _executingActions = false;
                    break;
                }
            }

            _executingActions = false;
        }
    }
}