using UnityEngine;

namespace Aspekt.AI
{
    public interface IAIAgent<L, V>
    {
        /// <summary>
        /// The memory component of the AI agent
        /// </summary>
        IMemory<L, V> Memory { get; }
        
        /// <summary>
        /// The Transform of the AI agent
        /// </summary>
        Transform Transform { get; }
        
        /// <summary>
        /// The Owner of the AI agent
        /// </summary>
        GameObject Owner { get; }
        
        /// <summary>
        /// The action controller of the AI agent
        /// </summary>
        IActionController<L, V> Actions { get; }
        
        /// <summary>
        /// The sensor controller of the AI agent
        /// </summary>
        ISensorController<L, V> Sensors { get; }
        
        /// <summary>
        /// The goals controller of the AI agent
        /// </summary>
        IGoalController<L, V> Goals { get; }
        
        /// <summary>
        /// Initialises the agent
        /// </summary>
        void Init(GameObject owner);

        /// <summary>
        /// Tells the agent to start operation, or resume if in paused state
        /// </summary>
        void Run();

        /// <summary>
        /// Pauses operation of the AI agent.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops the agent's AI completely.
        /// </summary>
        void Stop();
    }
}