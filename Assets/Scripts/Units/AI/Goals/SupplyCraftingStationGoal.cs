using Aspekt.AI;

namespace Aspekt.Drones
{
    public class SupplyCraftingStationGoal : AIGoal<AIAttributes, object>
    {
        public SupplyCraftingStationGoal(int priority) : base(priority)
        {
        }
        
        public override void SetupGoal()
        {
            // use sensor to find closest crafting station that requires resources,
            // as well as the available resources
        }

        public override void ResetGoal()
        {
            agent.Memory.Remove(AIAttributes.SupplyCraftingStationGoal);
        }

        protected override void SetConditions()
        {
            AddCondition(AIAttributes.SupplyCraftingStationGoal, true);
        }
    }
}