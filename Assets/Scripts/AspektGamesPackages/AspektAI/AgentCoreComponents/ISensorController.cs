using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI.Core
{
    /// <summary>
    /// Manages the AI agent's sensors
    /// </summary>
    public interface ISensorController<T, R>
    {
        /// <summary>
        /// Initialises the sensor controller
        /// </summary>
        /// <param name="agent">The parent AI agent</param>
        /// <param name="memory">the AI's memory module</param>
        void Init(IAIAgent<T, R> agent, IMemory<T, R> memory);
        
        /// <summary>
        /// Tick is called once per frame via the AI agent
        /// </summary>
        /// <param name="deltaTime">The time since the last frame</param>
        void Tick(float deltaTime);
        
        /// <summary>
        /// Returns a copy of the list of sensors in the sensor controller
        /// </summary>
        List<ISensor<T, R>> GetSensors();
        
        /// <summary>
        /// Disables all the sensors on the agent
        /// </summary>
        void DisableSensors();
        
        /// <summary>
        /// Enables all the sensors on the agent
        /// </summary>
        void EnableSensors();
        
        /// <summary>
        /// Adds a sensor of the given type to the agent
        /// </summary>
        /// <typeparam name="TSensor">The sensor type to add</typeparam>
        void AddSensor<TSensor>() where TSensor : ISensor<T, R>, new();
        
        /// <summary>
        /// Removes a sensor of the given type, if it exists on the agent
        /// </summary>
        /// <typeparam name="TSensor">The sensor type to remove</typeparam>
        void RemoveSensor<TSensor>() where TSensor : ISensor<T, R>;
    }
}