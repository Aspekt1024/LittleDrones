using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI.Core
{
    // Manages the operation of the agent's actions
    public class StateMachine<T, R> : IStateMachine<T, R>
    {
        private IAIAgent<T, R> agent;

        private Queue<IAIAction<T, R>> queue;
        private IAIAction<T, R> currentAction;
        
        private enum States
        {
            Paused, Stopped, Running
        }
        private States state = States.Stopped;

        public void Init(IAIAgent<T, R> agent)
        {
            this.agent = agent;
        }

        public void SetQueue(Queue<IAIAction<T, R>> newQueue)
        {
            queue = newQueue;
        }
        
        public bool Start()
        {
            state = States.Running;
            if (queue == null || queue.Count <= 0)
            {
                Debug.Log("cannot start machine with no queue");
                return false;
            }

            return true;
        }

        public void Stop()
        {
            state = States.Stopped;
        }

        public void Pause()
        {
            if (state == States.Paused || state == States.Stopped) return;
            state = States.Paused;
            
        }

        public void Tick(float deltaTime)
        {
            if (state == States.Paused || state == States.Stopped) return;

            if (currentAction == null || currentAction.IsComplete())
            {
                currentAction = queue.Dequeue();
                var success = currentAction.Begin();
                if (!success)
                {
                    Debug.Log("failed to begin action");
                    return;
                }
            }
            
            currentAction.Tick(deltaTime);

            if (currentAction.IsComplete())
            {
                Debug.Log("action complete. now what?");
            }
        }
    }
}