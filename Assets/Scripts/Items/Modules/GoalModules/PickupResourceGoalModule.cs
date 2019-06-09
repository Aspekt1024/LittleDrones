using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Pickup Resource Goal", menuName = "Drone/Goal Module/Pickup Resource")]
    public class PickupResourceGoalModule : GoalModule
    {
        public ResourceTypes resourceType;
        
        public override bool IsTypeMatch(GoalModule other)
        {
            if (other is PickupResourceGoalModule g)
            {
                return g.resourceType == resourceType;
            }
            return false;
        }

        protected override AIGoal<AIAttributes, object> CreateGoal()
        {
            return new PickUpResourceGoal(resourceType);
        }
    }
}