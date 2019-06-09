using System.Collections.Generic;

namespace Aspekt.AI
{
    public interface IAIGoal<L, V>
    {
        /// <summary>
        /// The priority of the goal. Higher values have higher priority
        /// </summary>
        float Priority { get; }

        /// <summary>
        /// Returns true if the goal is enabled
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// Initialises the goal
        /// </summary>
        void Init(IAIAgent<L, V> agent);
        
        /// <summary>
        /// Retrieves the conditions that meet the goal
        /// </summary>
        /// <returns></returns>
        Dictionary<L, V> GetConditions();

        /// <summary>
        /// Sets the priority for the goal (higher value has higher priority)
        /// </summary>
        /// <param name="priority">The priority value (higher value has higher priority)</param>
        void SetPriority(float priority);

        /// <summary>
        /// Enables the goal. The goal will be regarded when calculating new goals
        /// </summary>
        void Enable();

        /// <summary>
        /// Disables the goal. The goal will be disregarded when calculating new goals
        /// </summary>
        void Disable();

        /// <summary>
        /// Sets the AI agent's goal state
        /// </summary>
        void SetupGoal();
    }
}