using UnityEngine;

namespace Aspekt.AI
{
    public interface IAIAgent<T, R>
    {
        /// <summary>
        /// Initialises the agent
        /// </summary>
        void Init();
        
        /// <summary>
        /// Forces recalculation of the action path for the allocated goals
        /// </summary>
        void RefreshActions();

        /// <summary>
        /// Tells the agent to start operation, or resume if in paused state
        /// </summary>
        void Run();

        /// <summary>
        /// Pauses operation of the agent.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops the agent's AI completely.
        /// </summary>
        void Stop();

        /// <summary>
        /// Retrieves a sensor for the sensor module existing on the agent
        /// </summary>
        /// <typeparam name="TSensor">The sensor type</typeparam>
        /// <returns>The sensor module</returns>
        TSensor GetSensor<TSensor>();

        /// <summary>
        /// Returns the transform of the agent
        /// </summary>
        /// <returns>The agent's transform</returns>
        Transform GetTransform();

    }
}