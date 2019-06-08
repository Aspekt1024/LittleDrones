using System.Collections.Generic;
using System.Linq;

namespace Aspekt.AI
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

        public void AddGoal<TGoal>() where TGoal : IAIGoal<L, V>, new()
        {
            if (goals.Any(s => s is TGoal)) return;
            
            var goal = new TGoal();
            goal.Init(agent);
            goals.Add(goal);
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

        public void RemoveGoal<TGoal>() where TGoal : IAIGoal<L, V>
        {
            goals.RemoveAll(g => g is TGoal);
        }

        public void RemoveGoal(IAIGoal<L, V> goal)
        {
            goals.Remove(goal);
        }
    }
}