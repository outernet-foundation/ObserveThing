using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObserveThing
{
    public abstract class SynchronizationContext
    {
        public static SynchronizationContext Default = new DefaultSynchronizationContext();
        public abstract void EnqueueAction(Action action);
    }

    public class DefaultSynchronizationContext : SynchronizationContext
    {
        private const int MAX_NESTED_ENQUEUES = 100;

        private Queue<Action> _actionQueue = new Queue<Action>();
        private bool _executingActions = false;
        private bool _enqueuedDuringExecute = false;
        private int _executionPauses = 0;

        public void PauseExecution()
        {
            _executionPauses++;
        }

        public void ResumeExecution()
        {
            bool wasPaused = _executionPauses > 0;
            _executionPauses = Mathf.Max(0, _executionPauses - 1);
            if (_executionPauses == 0 && wasPaused)
                ExecutePendingActions();
        }

        public override void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);

            if (_executingActions)
            {
                _enqueuedDuringExecute = true;
                return;
            }

            if (_executionPauses > 0)
                return;

            ExecutePendingActions();
        }

        private void ExecutePendingActions()
        {
            _executingActions = true;

            int nestedEnqueues = 0;

            while (_actionQueue.TryDequeue(out var nextAction))
            {
                if (nestedEnqueues >= MAX_NESTED_ENQUEUES)
                {
                    _executingActions = false;
                    throw new Exception("Max nested enqueues exceeded. Could this be an infinite loop?");
                }

                nextAction?.Invoke();

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