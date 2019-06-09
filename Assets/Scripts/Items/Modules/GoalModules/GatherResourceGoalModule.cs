using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Gather Resource Goal", menuName = "Drone/Goal Module/Gather Resource")]
    public class GatherResourceGoalModule : GoalModule
    {
        public ResourceTypes resourceType;

        protected override AIGoal<AIAttributes, object> CreateGoal()
        {
            return new GatherResourceGoal(resourceType);;
        }
        
        public override bool IsTypeMatch(GoalModule other)
        {
            if (other is GatherResourceGoalModule g)
            {
                return g.resourceType == resourceType;
            }
            return false;
        }
    }
}