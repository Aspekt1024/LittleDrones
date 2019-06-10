using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI.Internal
{
    public interface IGoalController<L, V>
    {
        /// <summary>
        /// Initialises the goal controller
        /// </summary>
        /// <param name="agent">The parent AI agent</param>
        void Init(IAIAgent<L, V> agent);
        
        /// <summary>
        /// Returns the list of goals in the goal controller
        /// </summary>
        List<IAIGoal<L, V>> GetGoals();

        /// <summary>
        /// Returns a goal of the given type if it exists on the AI agent
        /// </summary>
        TGoal Get<TGoal>();
        
        /// <summary>
        /// Enables all the goals on the agent
        /// </summary>
        void EnableGoals();

        /// <summary>
        /// Adds the given goal to the agent. Checks if a goal of that type exists on the AI agent already
        /// </summary>
        void AddGoal(IAIGoal<L, V> goal);

        /// <summary>
        /// Removes the goal of the given type
        /// </summary>
        /// <param name="goal"></param>
        void RemoveGoal(IAIGoal<L, V> goal);
    }
}