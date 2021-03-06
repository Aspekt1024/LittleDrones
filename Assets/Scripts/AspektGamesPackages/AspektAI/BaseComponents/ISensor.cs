
using System.Collections.Generic;

namespace Aspekt.AI
{
    /// <summary>
    /// A sensor gives an agent the ability to perceive the world around it, or reflect upon itself.
    /// Sensors typically modify the agent's memory based on what they perceive.
    /// </summary>
    public interface ISensor<L, V>
    {
        /// <summary>
        /// True if the sensor is enabled
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// Initialises the sensor
        /// </summary>
        /// <param name="agent">The parent AI agent</param>
        void Init(IAIAgent<L, V> agent);

        /// <summary>
        /// Tick is called once per frame, similar to MonoBehaviour.Update()
        /// </summary>
        /// <param name="deltaTime">The time since the last frame</param>
        void Tick(float deltaTime);

        /// <summary>
        /// Enables the sensor
        /// </summary>
        void Enable();
        
        /// <summary>
        /// Disables the sensor
        /// </summary>
        void Disable();
        
        /// <summary>
        /// Used to clean up if the sensor affects memory
        /// </summary>
        void Remove();
    }
}