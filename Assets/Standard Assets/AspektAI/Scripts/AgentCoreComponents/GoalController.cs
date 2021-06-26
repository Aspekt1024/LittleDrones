using System.Collections.Generic;
using System.Linq;

namespace Aspekt.AI.Internal
{
    public class GoalController<L, V> : IGoalController<L, V>
    {
        private IAIAgent<L, V> agent;
        
        private readonly List<IAIGoal<L, V>> goals = new List<IAIGoal<L, V>>();
        
        public void Init(IAIAgent<L, V> agent)
        {
            this.agent = agent;
        }

        public List<IAIGoal<L, V>> GetGoals()
        {
            return goals;
        }

        public TGoal Get<TGoal>()
        {
            foreach (var goal in goals)
            {
                if (goal is TGoal g) return g;
            }
            return default;
        }

        public void EnableGoals()
        {
            foreach (var goal in goals)
            {
                goal.Enable();
            }
        }

        public void AddGoal(IAIGoal<L, V> goal)
        {
            var matchedGoals = goals.Where(g => g is L).ToArray();
            foreach (var matchedGoal in matchedGoals)
            {
                RemoveGoal(matchedGoal);
            }
            goal.Init(agent);
            goals.Add(goal);
        }

        public void RemoveGoal(IAIGoal<L, V> goal)
        {
            goals.Remove(goal);
        }

        public void Clear()
        {
            goals.Clear();
        }
    }
}