using System;
using System.Collections.Generic;

namespace Aspekt.AI.Internal
{
    public interface IPlanner<L, V>
    {
        /// <summary>
        /// Invoked when a new action plan has been found 
        /// </summary>
        event Action OnActionPlanFound;

        /// <summary>
        /// Invoked when finding a new action plan failed
        /// </summary>
        event Action OnActionPlanNotFound;
        
        /// <summary>
        /// Calculates a new goal based on the AI agent's available actions
        /// </summary>
        void CalculateNewGoal();

        void CalculateNewGoalDryRun(Action<ActionPlan<L, V>> callback);

        /// <summary>
        /// Retrieves the action plan found by the planner
        /// </summary>
        Queue<IAIAction<L, V>> GetActionPlan();

        /// <summary>
        /// Retrieves the goal that the action plan aims to fulfil
        /// </summary>
        IAIGoal<L, V> GetGoal();
    }
}