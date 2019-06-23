using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Maintain Fuel Goal", menuName = "Drone/Goal Module/Maintain Fuel")]
    public class MaintainFuelGoalModule : GoalModule
    {
        public override bool IsTypeMatch(GoalModule other)
        {
            return other is MaintainFuelGoalModule;
        }

        protected override AIGoal<AIAttributes, object> CreateGoal()
        {
            return new MaintainFuelGoal();
        }
    }
}