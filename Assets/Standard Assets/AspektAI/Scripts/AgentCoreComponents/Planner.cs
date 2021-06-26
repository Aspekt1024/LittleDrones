using System;
using System.Collections.Generic;
using Aspekt.AI.Planning;

namespace Aspekt.AI.Internal
{
    public class Planner<L, V> : IPlanner<L, V>
    {
        private readonly IAIAgent<L, V> agent;
        private readonly IAStar<L, V> aStar = new AStar<L, V>();
        
        private IAIGoal<L, V> currentGoal;
        private Queue<IAIAction<L, V>> actions = new Queue<IAIAction<L, V>>();

        private bool isDiagnosticsEnabled;
        private PlannerDiagnosticData<L, V> diagnosticData;
        public PlannerDiagnosticData<L, V> GetDiagnostics() => diagnosticData;

        public event Action OnActionPlanFound = delegate { };
        public event Action OnActionPlanNotFound = delegate { };

        private class NoActionPlanFoundException : Exception { }
        
        public Planner(IAIAgent<L, V> agent)
        {
            this.agent = agent;
        }
        
        public void CalculateNewGoal()
        {
            try
            {
                var actionPlan = CalculateActionPlan();
                currentGoal = actionPlan.Goal;
                actions = actionPlan.Actions;
            }
            catch (NoActionPlanFoundException e)
            {
                agent.LogInfo(this, "No action plan found: " + e.Message);
                currentGoal = null;
                actions.Clear();
                OnActionPlanNotFound?.Invoke();
                return;
            }

            if (actions.Count > 0)
            {
                OnActionPlanFound?.Invoke();
            }
            else
            {
                agent.LogKeyInfo(this, "unable to achieve any new goals");
                OnActionPlanNotFound?.Invoke();
            }
        }

        public void CalculateNewGoalDryRun(Action<ActionPlan<L, V>> callback)
        {
            try
            {
                var actionPlan = CalculateActionPlan();
                callback.Invoke(actionPlan);
            }
            catch (NoActionPlanFoundException e)
            {
                agent.LogInfo(this, "No action plan found: " + e.Message);
                callback.Invoke(new ActionPlan<L, V>());
            }
        }

        private ActionPlan<L, V> CalculateActionPlan()
        {
            diagnosticData = new PlannerDiagnosticData<L, V>();
            
            var goals = new List<IAIGoal<L, V>>(agent.Goals.GetGoals());
            goals.Sort((x, y) => y.Priority.CompareTo(x.Priority));

            if (goals.Count == 0)
            {
                agent.LogKeyInfo(this, "no goals found to plan for.");
                throw new NoActionPlanFoundException();
            }

            foreach (var goal in goals)
            {
                diagnosticData.BeginGoalInfo(goal);

                if (IsAlreadyAchieved(goal) || !IsGoalAchieveableByActions(goal))
                {
                    diagnosticData.EndGoalInfo(false, new ActionPlan<L, V>());
                    continue;
                }

                goal.SetupGoal();
                if (aStar.FindActionPlan(agent, goal))
                {
                    var actionPlan = aStar.GetActionPlan();
                    agent.LogKeyInfo(this, "action plan found for " + goal);
                    if (isDiagnosticsEnabled) diagnosticData.AddGoalMessage("Action plan found");
                    var plan = new ActionPlan<L, V> {Goal = goal, Actions = actionPlan};
                    diagnosticData.EndGoalInfo(true, plan);
                    return plan;
                }
                agent.LogInfo(this, "failed to find action plan for " + goal);
                if (isDiagnosticsEnabled) diagnosticData.AddGoalMessage("Unable to find action plan from available actions.");
                diagnosticData.EndGoalInfo(false, new ActionPlan<L, V>());
            }
            
            throw new NoActionPlanFoundException();
        }

        public Queue<IAIAction<L, V>> GetActionPlan() => actions;
        public IAIGoal<L, V> GetGoal() => currentGoal;

        public void SetDiagnosticsStatus(bool isActive)
        {
#if UNITY_EDITOR
            // Diagnostics is only relevant in the editor, so we'll keep it disabled in builds.
            isDiagnosticsEnabled = isActive;
#endif
        }

        private bool IsAlreadyAchieved(IAIGoal<L, V> goal)
        {
            foreach (var condition in goal.GetConditions())
            {
                if (!agent.Memory.IsMatch(condition.Key, condition.Value)) return false;
            }
            if (isDiagnosticsEnabled) diagnosticData.AddGoalMessage("Goal conditions have already been achieved.");
            return true;
        }

        private bool IsGoalAchieveableByActions(IAIGoal<L, V> goal)
        {
            // Checks if a goal is obtainable by any of the available actions (and sensors) on the agent.
            // If no actions on the agent meet the goal, the goal is deemed not achievable at this point.
            // If the goal is achievable, the actions' preconditions are checked by the AStar algorithm

            LogGoalConditionsTrace(goal);
            var conditions = goal.GetConditions();

            var conditionsMet = new Dictionary<L, bool>();
            foreach (var condition in conditions)
            {
                conditionsMet.Add(condition.Key, false);
            }

            foreach (var stateValue in agent.Memory.GetState())
            {
                if (conditionsMet.ContainsKey(stateValue.Key) && conditionsMet[stateValue.Key].Equals(conditions[stateValue.Key]))
                {
                    conditionsMet[stateValue.Key] = true;
                }
            }

            foreach (var action in agent.Actions.GetActions())
            {
                agent.LogTrace(this, $"checking if {action} meets goal conditions...");
                //if (!action.IsEnabled()) continue;

                foreach (var effect in action.GetEffects())
                {
                    agent.LogTrace(this, $"Effect: {effect.Key} : {effect.Value}");
                    if (conditionsMet.ContainsKey(effect.Key) && effect.Value.Equals(conditions[effect.Key]))
                    {
                        agent.LogTrace(this, $"Meets condition: {effect.Key} : {effect.Value}");
                        conditionsMet[effect.Key] = true;
                    }
                }
            }

            if (!conditionsMet.ContainsValue(false)) return true;
            LogGoalUnachievableTrace(goal, conditionsMet);
            return false;
        }

        private void LogGoalConditionsTrace(IAIGoal<L, V> goal)
        {
            if (agent.Logger.LogLevel != AILogger.LogLevels.Trace) return;
            
            string message = $"checking if {goal} can be achieved. Conditions:";
            foreach (var condition in goal.GetConditions())
            {
                message += $"\n{condition.Key} : {condition.Value}";
            }
            agent.LogTrace(this, message);
        }

        private void LogGoalUnachievableTrace(IAIGoal<L, V> goal, Dictionary<L, bool> conditionsMet)
        {
            if (isDiagnosticsEnabled) diagnosticData.AddGoalMessage("Goal condition is not achievable by any of its actions.");
            if (agent.Logger.LogLevel != AILogger.LogLevels.Trace) return;
            
            agent.LogTrace(this, $"{goal} not achievable by the available actions");
            foreach (var c in conditionsMet)
            {
                agent.LogTrace(this, $"{c.Key} : {c.Value}");
            }
        }
    }
}