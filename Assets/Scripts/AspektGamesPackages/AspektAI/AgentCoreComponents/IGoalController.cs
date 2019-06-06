using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI
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
        /// Adds an action of the given type to the agent
        /// </summary>
        /// <typeparam name="TGoal">The sensor type to add</typeparam>
        void AddGoal<TGoal>() where TGoal : IAIGoal<L, V>, new();
        
        /// <summary>
        /// Removes an action of the given type, if it exists on the agent
        /// </summary>
        /// <typeparam name="TGoal">The sensor type to remove</typeparam>
        void RemoveGoal<TGoal>() where TGoal : IAIGoal<L, V>;
    }
}