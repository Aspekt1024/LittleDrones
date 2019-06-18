using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Drones
{
    public class ResourceDepot : BuildingBase, IStorage
    {
        public List<ResourceTypes> resourceTypes;
        
        private void Awake()
        {
            buildingType = BuildingTypes.ResourceDepot;
        }

        public bool TakeItem(IGrabbableItem item)
        {
            if (!(item is ResourceBase resource)) return false;
            if (!resourceTypes.Contains(resource.resourceType)) return false;
            
            Destroy(resource.gameObject);
            
            // TODO setup resources
            Debug.Log("collected resource: " + resource.resourceType);
            
            return true;
        }
    }
}