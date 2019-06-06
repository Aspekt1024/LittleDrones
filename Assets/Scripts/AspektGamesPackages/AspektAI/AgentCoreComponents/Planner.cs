using System.Collections.Generic;

namespace Aspekt.AI
{
    public class Planner<L, V> : IPlanner<L, V>
    {
        private readonly IAIAgent<L, V> agent;
        private readonly IAIAStar<L, V> aStar = new AIAStar<L, V>();
        
        private IAIGoal<L, V> currentGoal;
        private Queue<IAIAction<L, V>> actions = new Queue<IAIAction<L, V>>();
        
        public Planner(IAIAgent<L, V> agent)
        {
            this.agent = agent;
        }

        public void CalculateNewGoal()
        {
            var goals = new List<IAIGoal<L, V>>(agent.Goals.GetGoals());
        }
    }
}