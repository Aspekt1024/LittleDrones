using System.Collections.Generic;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class GatherResourceGoal : AIGoal<AIAttributes, object>
    {
        private readonly ResourceTypes resourceType;
        
        private readonly Dictionary<ResourceTypes, AIAttributes> resourceDict = new Dictionary<ResourceTypes, AIAttributes>()
        {
            { ResourceTypes.Coal, AIAttributes.HasGatheredCoal },
            { ResourceTypes.Iron, AIAttributes.HasGatheredIron },
            { ResourceTypes.Copper, AIAttributes.HasGatheredCopper },
        };

        public GatherResourceGoal(ResourceTypes type)
        {
            resourceType = type;
        }
        
        public override void SetupGoal()
        {
            var attribute = GetAttribute();
            if (attribute == AIAttributes.Invalid) return;
            agent.Memory.Set(attribute, false);
        }
        
        protected override void SetConditions()
        {
            AddCondition(GetAttribute(), true);
        }

        private AIAttributes GetAttribute()
        {
            if (resourceDict.ContainsKey(resourceType))
            {
                return resourceDict[resourceType];
            }
            
            Debug.LogError("resource type not found in dictionary: " + resourceType);
            return AIAttributes.Invalid;
        }
    }
}