using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Aspekt.AI.Internal
{
    // Manages the AI agent's sensors
    public class SensorController<L, V> : ISensorController<L, V>
    {
        private IAIAgent<L, V> agent;
        
        private readonly List<ISensor<L, V>> sensors = new List<ISensor<L, V>>();

        private enum States
        {
            NotInitialised, NotInitialisedAlerted,
            Enabled, Disabled
        }

        private States state = States.NotInitialised;

        public void Init(IAIAgent<L, V> agent)
        {
            this.agent = agent;

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

        public List<ISensor<L, V>> GetSensors() => sensors;

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

        public void AddSensor(ISensor<L, V> sensor)
        {
            var matchedSensors = sensors.Where(g => g is L).ToArray();
            foreach (var matchedSensor in matchedSensors)
            {
                RemoveSensor(matchedSensor);
            }
            sensor.Init(agent);
            sensors.Add(sensor);
        }

        public void RemoveSensor(ISensor<L, V> sensor)
        {
            sensors.Remove(sensor);
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