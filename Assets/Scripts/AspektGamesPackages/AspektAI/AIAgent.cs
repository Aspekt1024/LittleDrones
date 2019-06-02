using Aspekt.AI.Core;
using UnityEngine;

namespace Aspekt.AI
{
    // Attached to a game object to give it the power of thought
    public class AIAgent : MonoBehaviour, IAIAgent
    {
        public readonly ISensorController Sensors = new SensorController();
        public readonly IMemory Memory = new Memory();
        public readonly IStateMachine StateMachine = new StateMachine();

        private enum States
        {
            NotInitialised, Running, Stopped, Paused
        }

        private States state = States.NotInitialised;

        public void Init()
        {
            state = States.Stopped;
            
            Memory.Init(this);
            Sensors.Init(this, Memory);
            StateMachine.Init(this);
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