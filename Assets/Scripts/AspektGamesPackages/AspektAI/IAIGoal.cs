using System.Collections.Generic;

namespace Aspekt.AI
{
    public interface IAIGoal<L, V>
    {
        /// <summary>
        /// Returns true if the goal is enabled
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// Initialises the goal
        /// </summary>
        void Init();
        
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
    }
}