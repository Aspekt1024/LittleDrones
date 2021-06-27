using System.Collections.Generic;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class GatherResourceGoal : AIGoal<AIAttributes, object>
    {
        private readonly ResourceTypes resourceType;

        public GatherResourceGoal(ResourceTypes type, int priority) : base(priority)
        {
            resourceType = type;
        }

        public override void SetupGoal()
        {
            agent.Memory.Set(AIAttributes.ResourceGoalType, resourceType);
        }

        public override void ResetGoal()
        {
            agent.Memory.Remove(AIAttributes.GatherResourceGoal);
        }

        protected override void SetConditions()
        {
            AddCondition(AIAttributes.GatherResourceGoal, true);
        }
    }
}