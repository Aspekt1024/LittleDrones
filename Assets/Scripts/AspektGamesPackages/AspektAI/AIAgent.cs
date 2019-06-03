using Aspekt.AI.Core;
using UnityEngine;

namespace Aspekt.AI
{
    // Attached to a game object to give it the power of thought
    public abstract class AIAgent<T, R> : MonoBehaviour, IAIAgent<T, R>
    {
        public readonly IActionController<T, R> Actions = new ActionController<T, R>();
        public readonly ISensorController<T, R> Sensors = new SensorController<T, R>();
        public readonly IStateMachine<T, R> StateMachine = new StateMachine<T, R>();
        public readonly IMemory<T, R> Memory = new Memory<T, R>();

        private enum States
        {
            NotInitialised, Running, Stopped, Paused
        }

        private States state = States.NotInitialised;

        public void Init()
        {
            state = States.Stopped;
            
            Actions.Init(this, Memory);
            Sensors.Init(this, Memory);
            StateMachine.Init(this);
            Memory.Init(this);
        }
        
        private void Start()
        {
            if (state == States.NotInitialised)
            {
                Debug.LogError("Agent.Init() has not been called. This agent will not function.");
            }
        }

        private void Update()
        {
            if (state == States.Running)
            {
                Actions.Tick(Time.deltaTime);
                Sensors.Tick(Time.deltaTime);
                StateMachine.Tick(Time.deltaTime);
            }
        }

        public void RefreshActions()
        {
            
        }
        
        public void Run()
        {
            state = States.Running;
            // TODO get action list
        }

        public void Resume()
        {
            if (state != States.Paused)
            {
                Debug.LogWarning("Attempted to resume Agent from non-paused state. This has no effect");
                return;
            }

            state = States.Running;
        }

        public void Pause()
        {
            Sensors.DisableSensors();
            StateMachine.Pause();
        }

        public void Stop()
        {
            Sensors.DisableSensors();
            StateMachine.Stop();
            Memory.Reset();
        }
    }
}