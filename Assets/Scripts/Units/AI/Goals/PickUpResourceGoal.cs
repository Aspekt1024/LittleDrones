using System.Collections.Generic;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class PickUpResourceGoal : AIGoal<AIAttributes, object>
    {
        private readonly ResourceTypes resourceType;
        
        public PickUpResourceGoal(ResourceTypes type)
        {
            resourceType = type;
        }
        
        public override void SetupGoal()
        {
            agent.Memory.Set(AIAttributes.ResourceGoalType, resourceType);
        }

        public override void ResetGoal()
        {
            
        }

        protected override void SetConditions()
        {
            AddCondition(AIAttributes.IsHoldingItem, true);
        }
    }
}