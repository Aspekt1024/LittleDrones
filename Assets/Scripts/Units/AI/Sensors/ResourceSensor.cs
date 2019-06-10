using System;
using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [Serializable]
    public class ResourceSensor : Sensor<AIAttributes, object>
    {
        [Range(1f, 200f)] public float detectionRadius = 100f;
        
        /// <summary>
        /// The minimum amount of time that must pass before scanning can occur again
        /// A call to <see cref="ScanResources"/> prior to this will return the previous scan's results
        /// </summary>
        public float maxRefreshRate = 0.5f;

        private ResourceTypes resourceType = ResourceTypes.None;

        private ResourceBase[] resources;
        private float timeLastSensed;
        
        public ResourceBase[] ScanResources(ResourceTypes type)
        {
            if (Time.time < timeLastSensed + maxRefreshRate) return resources;
            
            var mask = 1 << LayerMask.NameToLayer("Resource");
            var colliders = Physics.OverlapSphere(Agent.Transform.position, detectionRadius, mask);
            
            Agent.LogInfo(this, $"found {colliders.Length} {type} resources");

            resources = colliders.Select(c => c.GetComponentInParent<ResourceBase>()).Where(r => r.resourceType == type).ToArray();
            timeLastSensed = Time.time;
            return resources;
        }

        public ResourceBase GetClosestResource(ResourceTypes type, Vector3 pos)
        {
            resources = ScanResources(type);

            float dist = float.MaxValue;
            ResourceBase closestResource = null;
            foreach (var resource in resources)
            {
                if (!resource.gameObject.activeSelf) continue;
                var d = Vector3.Distance(pos, resource.transform.position);
                if (d >= dist) continue;
                
                closestResource = resource;
                dist = d;
            }

            return closestResource;
        }
        
        protected override void OnInit()
        {
            timeLastSensed = Time.time;
        }

        protected override void OnTick(float deltaTime)
        {
            if (Time.time > timeLastSensed + maxRefreshRate)
            {
                timeLastSensed = Time.time;
                var resources = ScanResources(resourceType);
                // TODO set in memory?
            }
        }
    }
}