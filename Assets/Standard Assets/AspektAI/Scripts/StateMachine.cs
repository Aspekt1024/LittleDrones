using System;
using System.Collections.Generic;

namespace Aspekt.AI
{
    // Manages the operation of the agent's actions
    public class StateMachine<L, V> : IStateMachine<L, V>
    {
        private readonly IAIAgent<L, V> agent;

        private readonly Queue<IMachineState<L, V>> stateQueue = new Queue<IMachineState<L, V>>();
        private IMachineState<L, V> currentState;
        
        public event Action OnComplete = delegate { };
        
        private enum States
        {
            Paused, Stopped, Running, Idle
        }
        private States state = States.Stopped;

        public StateMachine(IAIAgent<L, V> agent)
        {
            this.agent = agent;
        }

        public void Enqueue(IMachineState<L, V> machineState)
        {
            stateQueue.Enqueue(machineState);
        }

        public void Start()
        {
            state = States.Running;
        }

        public void Stop()
        {
            state = States.Stopped;
            if (currentState != null)
            {
                currentState.Stop();
                currentState.OnComplete -= OnStateCompleted;
                currentState = null;
            }

            stateQueue.Clear();
            OnComplete = null;
        }

        public void Pause()
        {
            if (state == States.Paused || state == States.Stopped) return;
            state = States.Paused;
            currentState?.Pause();
        }

        public void Tick(float deltaTime)
        {
            if (state == States.Paused || state == States.Stopped) return;

            if (currentState == null)
            {
                GotoNextState();
            }
            else
            {
                currentState.Tick(deltaTime);
            }
        }
        
        private void GotoNextState()
        {
            if (currentState != null)
            {
                currentState.OnComplete -= OnStateCompleted;
            }

            if (stateQueue.Count == 0) return;
            
            currentState = stateQueue.Dequeue();
            currentState.OnComplete += OnStateCompleted;
            currentState.Start();
        }

        private void OnStateCompleted()
        {
            if (state != States.Running) return;
            
            if (stateQueue.Count == 0)
            {
                OnComplete?.Invoke();
                if (currentState != null) currentState.OnComplete -= OnStateCompleted;
            }
            GotoNextState();
        }
    }
}