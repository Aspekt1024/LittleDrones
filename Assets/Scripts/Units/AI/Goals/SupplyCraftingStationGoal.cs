using Aspekt.AI;

namespace Aspekt.Drones
{
    public class SupplyCraftingStationGoal : AIGoal<AIAttributes, object>
    {
        public override void SetupGoal()
        {
            // use sensor to find closest crafting station that requires resources,
            // as well as the available resources
            
        }

        public override void ResetGoal()
        {
            agent.Memory.Remove(AIAttributes.HasSuppliedCraftingStation);
        }

        protected override void SetConditions()
        {
            AddCondition(AIAttributes.HasSuppliedCraftingStation, true);
        }
    }
}