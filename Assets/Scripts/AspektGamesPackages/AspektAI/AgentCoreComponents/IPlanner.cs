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
        /// Calculates a new goal based on the AI agent's available actions
        /// </summary>
        void CalculateNewGoal();

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