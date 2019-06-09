using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.AI.Planning;
using UnityEngine;

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

            if (goals.Count == 0)
            {
                agent.LogKeyInfo(this, "no goals found to plan for.");
                return;
            }
            
            for (int i = 0; i < goals.Count; i++)
            {
                currentGoal = goals[i];
                if (!IsGoalAchieveableByActions(currentGoal)) continue;

                if (aStar.FindActionPlan(agent, goals[i]))
                {
                    actions = aStar.GetActionPlan();
                    agent.LogKeyInfo(this, "action plan found");
                }
                else
                {
                    agent.LogKeyInfo(this, "failed to find action plan.");
                }
            }

            if (actions.Count > 0)
            {
                OnActionPlanFound?.Invoke();
            }
            else
            {
                agent.LogKeyInfo(this, "action plan is empty");
            }
        }

        public Queue<IAIAction<L, V>> GetActionPlan() => actions;
        public IAIGoal<L, V> GetGoal() => currentGoal;

        private bool IsGoalAchieveableByActions(IAIGoal<L, V> goal)
        {
            // Checks if a goal is obtainable by any of the available actions (and sensors) on the agent.
            // If no actions on the agent meet the goal, the goal is deemed not achievable at this point.
            // If the goal is achievable, the actions' preconditions are checked by the AStar algorithm

            string message = $"checking if {goal} can be achieved. Conditions:";
            foreach (var condition in goal.GetConditions())
            {
                message += $"\n{condition.Key} : {condition.Value}";
            }
            agent.LogTrace(this, message);

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
                agent.LogTrace(this, $"checking if {action} meets goal conditions...");
                if (!action.CheckProceduralPreconditions()) continue;

                var effects = action.GetEffects();
                if (!effects.Any())
                {
                    Debug.LogError($"{action} has no effect.");
                }
                foreach (var effect in effects)
                {
                    agent.LogTrace(this, $"Effect: {effect.Key} : {effect.Value}");
                    if (conditionsMet.ContainsKey(effect.Key) && effect.Value.Equals(goal.GetConditions()[effect.Key]))
                    {
                        agent.LogTrace(this, $"Meets condition: {effect.Key} : {effect.Value}");
                        conditionsMet[effect.Key] = true;
                    }
                }
            }

            if (conditionsMet.ContainsValue(false))
            {
                agent.LogInfo(this, $"{goal} not achievable by the available actions");
                foreach (var c in conditionsMet)
                {
                    agent.LogInfo(this, $"{c.Key} : {c.Value}");
                }
                return false;
            }
            
            return true;
        }
    }
}