using System.Collections.Generic;

namespace Aspekt.AI.Planning
{
    public interface IAStar<L, V>
    {
        /// <summary>
        /// Calculates an action plan based on the agent's available goals and actions
        /// </summary>
        /// <param name="agent">The AI agent</param>
        /// <param name="goal">The goal to be assessed</param>
        /// <returns>True</returns>
        bool FindActionPlan(IAIAgent<L, V> agent, IAIGoal<L, V> goal);

        Queue<IAIAction<L, V>> GetActionPlan();
    }
}