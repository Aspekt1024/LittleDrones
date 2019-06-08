using System;
using System.Collections.Generic;
using Aspekt.AI.Planning;

namespace Aspekt.AI
{
    public class Planner<L, V> : IPlanner<L, V>
    {
        private readonly IAIAgent<L, V> agent;
        private readonly IAStar<L, V> aStar = new AStar<L, V>();
        
        private IAIGoal<L, V> currentGoal;
        private Queue<IAIAction<L, V>> actions = new Queue<IAIAction<L, V>>();
        
        public event Action OnActionPlanFound = delegate { };
        
        public Planner(IAIAgent<L, V> agent)
        {
            this.agent = agent;
        }

        public void CalculateNewGoal()
        {
            var goals = new List<IAIGoal<L, V>>(agent.Goals.GetGoals());
            goals.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            
            if (goals.Count == 0) return;
            
            for (int i = 0; i < goals.Count; i++)
            {
                currentGoal = goals[i];
                if (!IsGoalAchieveableByActions(currentGoal)) continue;

                if (aStar.FindActionPlan(agent, goals[i]))
                {
                    actions = aStar.GetActionPlan();
                }
                else
                {
                    agent.Log(AILogType.KeyInfo, "failed to find action plan.");
                }
            }

            if (actions.Count > 0)
            {
                OnActionPlanFound?.Invoke();
            }
        }

        public Queue<IAIAction<L, V>> GetActionPlan() => actions;
        public IAIGoal<L, V> GetGoal() => currentGoal;

        private bool IsGoalAchieveableByActions(IAIGoal<L, V> goal)
        {
            // Checks if a goal is obtainable by any of the available actions on the agent.
            // If no actions on the agent meet the goal, the goal is deemed not achievable at this point.
            // If the goal is achievable, the actions' preconditions are checked by the AStar algorithm

            var conditionsMet = new Dictionary<L, bool>();
            foreach (var condition in goal.GetConditions())
            {
                conditionsMet.Add(condition.Key, false);
            }

            foreach (var stateValue in agent.Memory.GetState())
            {
                if (conditionsMet.ContainsKey(stateValue.Key) && conditionsMet[stateValue.Key].Equals(goal.GetConditions()[stateValue.Key]))
                {
                    conditionsMet[stateValue.Key] = true;
                }
            }

            foreach (var action in agent.Actions.GetActions())
            {
                if (!action.CheckProceduralPreconditions()) continue;

                foreach (var effect in action.GetEffects())
                {
                    if (conditionsMet.ContainsKey(effect.Key) && effect.Value.Equals(goal.GetConditions()[effect.Key]))
                    {
                        conditionsMet[effect.Key] = true;
                    }
                }
            }

            return !conditionsMet.ContainsValue(false);
        }
    }
}