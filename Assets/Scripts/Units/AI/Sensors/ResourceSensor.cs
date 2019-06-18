using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [Serializable]
    public class ResourceSensor : ObjectSensorBase<ResourceBase>
    {
        protected override LayerMask ObjectLayerMask { get; set; }

        public ResourceBase[] ScanResources(ResourceTypes type)
        {
            return Scan(r => r.resourceType == type);
        }

        public ResourceBase GetClosestResource(ResourceTypes type, Vector3 pos)
        {
            return GetClosestObject(pos, r => r.resourceType == type);
        }
        
        protected override void OnInit()
        {
            ObjectLayerMask = 1 << LayerMask.NameToLayer("Resource");
        }

    }
}