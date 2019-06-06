using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Aspekt.AI
{
    // Manages the agent's sensors
    public class SensorController<L, V> : ISensorController<L, V>
    {
        private IAIAgent<L, V> agent;
        private IMemory<L, V> memory;
        
        private readonly List<ISensor<L, V>> sensors = new List<ISensor<L, V>>();

        private enum States
        {
            NotInitialised, NotInitialisedAlerted,
            Enabled, Disabled
        }

        private States state = States.NotInitialised;

        public void Init(IAIAgent<L, V> agent, IMemory<L, V> memory)
        {
            this.agent = agent;
            this.memory = memory;

            state = States.Enabled;
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

        public List<ISensor<L, V>> GetSensors() => new List<ISensor<L, V>>(sensors);

        public void AddSensor<TSensor>() where TSensor : ISensor<L, V>, new()
        {
            if (sensors.Any(s => s is L)) return;
            
            var sensor = new TSensor();
            sensor.Init(agent, memory);
            sensors.Add(sensor);
        }

        public void RemoveSensor<TSensor>() where TSensor : ISensor<L, V>
        {
            sensors.RemoveAll(s => s is TSensor);
        }

        public TSensor Get<TSensor>()
        {
            foreach (var sensor in sensors)
            {
                if (sensor is TSensor s) return s;
            }
            return default;
        }
        
        private bool CanTick()
        {
            if (state == States.NotInitialised)
            {
                Debug.LogError($"{nameof(SensorController<L, V>)}.{nameof(Init)}() has not been called.");
                state = States.NotInitialisedAlerted;
            }

            return state == States.Enabled;
        }
    }
}