using Aspekt.AI;

namespace Aspekt.Drones
{
    public class CraftDroneGoal : AIGoal<AIAttributes, object>
    {
        public CraftDroneGoal(int priority) : base(priority)
        {
        }
        
        public override void SetupGoal()
        {
            
        }

        public override void ResetGoal()
        {
            agent.Memory.Set(AIAttributes.HasCraftedDrone, false);
        }

        protected override void SetConditions()
        {
            AddCondition(AIAttributes.HasCraftedDrone, true);
        }
    }
}