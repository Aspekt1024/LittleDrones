using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Aspekt.AI.Core
{
    // Manages the agent's sensors
    public class SensorController : ISensorController
    {
        private IAIAgent agent;
        private IMemory memory;
        
        private List<ISensor> sensors = new List<ISensor>();

        private enum States
        {
            NotInitialised, NotInitialisedAlerted,
            Enabled, Disabled
        }

        private States state = States.NotInitialised;

        public void Init(IAIAgent agent, IMemory memory)
        {
            this.agent = agent;
            this.memory = memory;

            state = States.Enabled;

            if (agent is MonoBehaviour mb)
            {
                sensors = mb.GetComponentsInChildren<ISensor>().ToList();
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

        public List<ISensor> GetSensors() => new List<ISensor>(sensors);

        public void AddSensor<T>() where T : ISensor, new()
        {
            if (sensors.Any(s => s is T)) return;
            
            var sensor = new T();
            sensor.Init(agent, memory);
            sensors.Add(sensor);
        }

        public void RemoveSensor<T>() where T : ISensor
        {
            sensors.RemoveAll(s => s is T);
        }
        
        private bool CanTick()
        {
            if (state == States.NotInitialised)
            {
                Debug.LogError($"{nameof(SensorController)}.{nameof(Init)}() has not been called.");
                state = States.NotInitialisedAlerted;
            }

            return state == States.Enabled;
        }
    }
}