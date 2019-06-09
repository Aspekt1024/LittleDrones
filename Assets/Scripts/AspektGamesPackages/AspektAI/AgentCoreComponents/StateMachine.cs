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

        public void Enqueue(IMachineState<L, V> state)
        {
            stateQueue.Enqueue(state);
        }

        public T AddState<T>() where T : IMachineState<L, V>, new()
        {
            var newState = new T();
            newState.Init(agent);
            Enqueue(newState);
            return newState;
        }
        
        public void Start()
        {
            state = States.Running;
        }

        public void Stop()
        {
            state = States.Stopped;
            currentState?.Stop();
        }

        public void Pause()
        {
            if (state == States.Paused || state == States.Stopped) return;
            state = States.Paused;
            currentState.Pause();
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
        }

        private void OnStateCompleted()
        {
            if (state == States.Running && stateQueue.Count == 0)
            {
                OnComplete?.Invoke();
                if (currentState != null) currentState.OnComplete -= OnStateCompleted;
            }
            GotoNextState();
        }
    }
}