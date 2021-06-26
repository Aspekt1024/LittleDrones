using System;
using System.Collections.Generic;
using Aspekt.AI.Planning;

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

        /// <summary>
        /// Enables or disables Diagnostics for the AI Agent. This will keep track of detailed info
        /// during the planning stage for displaying in the editor.
        /// </summary>
        void SetDiagnosticsStatus(bool isActive);

        /// <summary>
        /// Returns the diagnostics data. Only valid if <see cref="SetDiagnosticsStatus"/> is called and set to true
        /// </summary>
        PlannerDiagnosticData<L, V> GetDiagnostics();
    }
}