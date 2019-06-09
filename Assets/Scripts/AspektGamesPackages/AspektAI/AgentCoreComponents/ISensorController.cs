using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI
{
    /// <summary>
    /// Manages the AI agent's sensors
    /// </summary>
    public interface ISensorController<L, V>
    {
        /// <summary>
        /// Initialises the sensor controller
        /// </summary>
        /// <param name="agent">The parent AI agent</param>
        void Init(IAIAgent<L, V> agent);
        
        /// <summary>
        /// Tick is called once per frame via the AI agent
        /// </summary>
        /// <param name="deltaTime">The time since the last frame</param>
        void Tick(float deltaTime);
        
        /// <summary>
        /// Returns a copy of the list of sensors in the sensor controller
        /// </summary>
        List<ISensor<L, V>> GetSensors();
        
        /// <summary>
        /// Disables all the sensors on the AI agent
        /// </summary>
        void DisableSensors();
        
        /// <summary>
        /// Enables all the sensors on the AI agent
        /// </summary>
        void EnableSensors();

        /// <summary>
        /// Adds the given sensor to the AI agent. Replaces sensors of the same type that exist on the AI agent already
        /// </summary>
        void AddSensor(ISensor<L, V> sensor);

        /// <summary>
        /// Removes the sensor from the AI agent
        /// </summary>
        /// <param name="sensor"></param>
        void RemoveSensor(ISensor<L, V> sensor);

        /// <summary>
        /// Retrieves a sensor of the given type if it exists on the AI agent
        /// </summary>
        TSensor Get<TSensor>();
    }
}