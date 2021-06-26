using System.Collections.Generic;

namespace Aspekt.AI
{
    public interface IAIGoal<L, V>
    {
        /// <summary>
        /// The priority of the goal. Higher values have higher priority
        /// </summary>
        float Priority { get; set; }

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
        /// Enables the goal. The goal will be regarded when calculating new goals
        /// </summary>
        void Enable();

        /// <summary>
        /// Disables the goal. The goal will be disregarded when calculating new goals
        /// </summary>
        void Disable();

        /// <summary>
        /// Sets the AI agent's goal state. Called during the planning stage, and before the goal begins execution.
        /// </summary>
        void SetupGoal();

        /// <summary>
        /// Used to reset the goal state once achieved
        /// </summary>
        void ResetGoal();
    }
}