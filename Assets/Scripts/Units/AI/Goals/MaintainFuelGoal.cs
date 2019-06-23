using Aspekt.AI;

namespace Aspekt.Drones
{
    public class MaintainFuelGoal : AIGoal<AIAttributes, object>
    {
        public override void SetupGoal()
        {
            agent.Memory.Set(AIAttributes.ResourceGoalType, ResourceTypes.Coal);
        }

        public override void ResetGoal()
        {
            // Not required - set by sensor
        }

        protected override void SetConditions()
        {
            AddCondition(AIAttributes.HasLowFuel, false);
        }
    }
}