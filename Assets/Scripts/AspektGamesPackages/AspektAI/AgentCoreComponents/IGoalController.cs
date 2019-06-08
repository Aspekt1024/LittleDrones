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
        /// Adds a goal of the given type to the agent
        /// </summary>
        /// <typeparam name="TGoal">The goal type to add</typeparam>
        void AddGoal<TGoal>() where TGoal : IAIGoal<L, V>, new();

        /// <summary>
        /// Adds the given goal to the agent. Checks if a goal of that type exists on the AI agent already
        /// </summary>
        void AddGoal(IAIGoal<L, V> goal);
        
        /// <summary>
        /// Removes a goal of the given type, if it exists on the agent
        /// </summary>
        /// <typeparam name="TGoal">The goal type to remove</typeparam>
        void RemoveGoal<TGoal>() where TGoal : IAIGoal<L, V>;

        /// <summary>
        /// Removes the goal of the given type
        /// </summary>
        /// <param name="goal"></param>
        void RemoveGoal(IAIGoal<L, V> goal);
    }
}