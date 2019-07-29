using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Drones
{
    public class ResourceDepot : BuildingBase, IStorage
    {
        public List<ResourceTypes> resourceTypes;
        
        private readonly Dictionary<ResourceTypes, int> resourceDict = new Dictionary<ResourceTypes, int>();
        
        private void Awake()
        {
            buildingType = BuildingTypes.ResourceDepot;
        }

        public bool TakeItem(IGrabbableItem item)
        {
            if (!(item is ResourceBase resource)) return false;
            if (!resourceTypes.Contains(resource.resourceType)) return false;
            
            Destroy(resource.gameObject);

            if (!resourceDict.ContainsKey(resource.resourceType))
            {
                resourceDict.Add(resource.resourceType, 1);
            }
            else
            {
                resourceDict[resource.resourceType]++;
            }
            
            return true;
        }

        public int GetResourceCount(ResourceTypes type)
        {
            return resourceDict.ContainsKey(type) ? resourceDict[type] : 0;
        }
    }
}