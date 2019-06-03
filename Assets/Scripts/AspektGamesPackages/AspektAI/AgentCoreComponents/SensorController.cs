using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Aspekt.AI.Core
{
    // Manages the agent's sensors
    public class SensorController<T, R> : ISensorController<T, R>
    {
        private IAIAgent<T, R> agent;
        private IMemory<T, R> memory;
        
        private List<ISensor<T, R>> sensors = new List<ISensor<T, R>>();

        private enum States
        {
            NotInitialised, NotInitialisedAlerted,
            Enabled, Disabled
        }

        private States state = States.NotInitialised;

        public void Init(IAIAgent<T, R> agent, IMemory<T, R> memory)
        {
            this.agent = agent;
            this.memory = memory;

            state = States.Enabled;

            if (agent is MonoBehaviour mb)
            {
                sensors = mb.GetComponentsInChildren<ISensor<T, R>>().ToList();
            }
            else
            {
                Debug.LogError("AI Agents must inherit from MonoBehaviour.");
            }
            
            foreach (var sensor in sensors)
            {
                sensor.Init(agent, memory);
            }
        }

        public void Tick(float deltaTime)
        {
            if (!CanTick()) return;
            
            foreach (var sensor in sensors)
            {
                sensor.Tick(deltaTime);
            }
        }

        public void DisableSensors()
        {
            if (state == States.Enabled)
            {
                state = States.Disabled;
            }
        }

        public void EnableSensors()
        {
            if (state == States.Disabled)
            {
                state = States.Enabled;
            }
            else if (state == States.NotInitialised || state == States.NotInitialisedAlerted)
            {
                Debug.LogError("Cannot enable non-initiated sensors.");
            }
        }

        public List<ISensor<T, R>> GetSensors() => new List<ISensor<T, R>>(sensors);

        public void AddSensor<TSensor>() where TSensor : ISensor<T, R>, new()
        {
            if (sensors.Any(s => s is T)) return;
            
            var sensor = new TSensor();
            sensor.Init(agent, memory);
            sensors.Add(sensor);
        }

        public void RemoveSensor<TSensor>() where TSensor : ISensor<T, R>
        {
            sensors.RemoveAll(s => s is TSensor);
        }
        
        private bool CanTick()
        {
            if (state == States.NotInitialised)
            {
                Debug.LogError($"{nameof(SensorController<T, R>)}.{nameof(Init)}() has not been called.");
                state = States.NotInitialisedAlerted;
            }

            return state == States.Enabled;
        }
    }
}