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
            bool TypeMatchPredicate(ResourceBase r) => r.resourceType == type;
            return Scan(TypeMatchPredicate);
        }

        public ResourceBase GetClosestResource(ResourceTypes type, Vector3 pos)
        {
            bool TypeMatchPredicate(ResourceBase r) => r.resourceType == type;
            return GetClosestObject(pos, TypeMatchPredicate);
        }
        
        protected override void OnInit()
        {
            ObjectLayerMask = 1 << LayerMask.NameToLayer("Resource");
        }

    }
}