using System.Collections.Generic;

namespace Aspekt.AI.Planning
{
    public class PlannerDiagnosticData<L, V>
    {
        public readonly List<GoalCalculationInfo> GoalInfo = new List<GoalCalculationInfo>();

        private GoalCalculationInfo goalInfo;

        public struct GoalCalculationInfo
        {
            public IAIGoal<L, V> Goal;
            public bool IsAchievable;
            public bool IsProcessed;
            public List<string> Messages;
            public List<IAIAction<L, V>> ViableActions;
            public List<string> NonViableActionDetails;
            public ActionPlan<L, V> ActionPlan;
        }

        public PlannerDiagnosticData()
        {
        }
        
        public PlannerDiagnosticData(PlannerDiagnosticData<L, V> original)
        {
            GoalInfo = new List<GoalCalculationInfo>(original.GoalInfo);
        }

        public void BeginGoalInfo(IAIGoal<L, V> goal)
        {
            goalInfo = new GoalCalculationInfo
            {
                Goal = goal,
                IsProcessed = false,
                Messages = new List<string>(),
                ViableActions = new List<IAIAction<L, V>>(),
                NonViableActionDetails = new List<string>(),
            };
        }

        public void AddGoalMessage(string message)
        {
            goalInfo.Messages.Add(message);
        }

        public void AddViableAction(IAIAction<L, V> action)
        {
            goalInfo.ViableActions.Add(action);
        }

        public void AddNonViableActionDetail(string detail)
        {
            goalInfo.NonViableActionDetails.Add(detail);
        }

        public void EndGoalInfo(bool isAchievable, ActionPlan<L, V> actionPlan)
        {
            goalInfo.IsAchievable = isAchievable;
            goalInfo.ActionPlan = actionPlan;
            GoalInfo.Add(goalInfo);
        }
    }
}