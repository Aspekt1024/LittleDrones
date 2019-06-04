using System.Collections.Generic;
using System.Linq;
using Aspekt.AI.Core;
using Aspekt.Drones;
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
                Sensors.Tick(Time.deltaTime);
                StateMachine.Tick(Time.deltaTime);
            }
        }
        
        public void Run()
        {
            if (state == States.NotInitialised)
            {
                Debug.LogError("Agent.Init() has not been called. This agent will not function.");
            }
            
            state = States.Running;
            RefreshActions();
        }

        public void RefreshActions()
        {
            var actions = Actions.GetActions().Where(a => a is GatherIronAction).ToArray();
            if (actions.Any())
            {
                var queue = new Queue<IAIAction<T, R>>();
                queue.Enqueue(actions[0]);
                StateMachine.SetQueue(queue);
                StateMachine.Start();
            }
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

        public TSensor GetSensor<TSensor>()
        {
            foreach (var sensor in Sensors.GetSensors())
            {
                if (sensor is TSensor s) return s;
            }
            return default;
        }

        public Transform GetTransform() => transform;
    }
}