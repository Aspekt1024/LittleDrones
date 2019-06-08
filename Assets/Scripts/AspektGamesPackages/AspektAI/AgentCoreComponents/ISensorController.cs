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
        /// Adds a sensor of the given type to the AI agent
        /// </summary>
        /// <typeparam name="TSensor">The sensor type to add</typeparam>
        void AddSensor<TSensor>() where TSensor : ISensor<L, V>, new();
        
        /// <summary>
        /// Removes a sensor of the given type, if it exists on the AI agent
        /// </summary>
        /// <typeparam name="TSensor">The sensor type to remove</typeparam>
        void RemoveSensor<TSensor>() where TSensor : ISensor<L, V>;

        /// <summary>
        /// Retrieves a sensor of the given type if it exists on the AI agent
        /// </summary>
        /// <typeparam name="TSensor"></typeparam>
        /// <returns></returns>
        TSensor Get<TSensor>();
    }
}