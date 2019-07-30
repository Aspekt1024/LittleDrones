using Aspekt.AI;

namespace Aspekt.Drones
{
    public class CraftDroneGoal : AIGoal<AIAttributes, object>
    {
        public override void SetupGoal()
        {
            
        }

        public override void ResetGoal()
        {
            agent.Memory.Remove(AIAttributes.HasCraftedDrone);
        }

        protected override void SetConditions()
        {
            AddCondition(AIAttributes.HasCraftedDrone, true);
        }
    }
}