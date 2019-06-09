using System;
using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [Serializable]
    public class ResourceSensor : Sensor<AIAttributes, object>
    {
        [Range(1f, 200f)]
        public float DetectionRadius = 100f;
        
        public float RefreshRate = 1f;

        private ResourceTypes resourceType = ResourceTypes.None;
        
        public enum Modes
        {
            PeriodicUpdate, OnDemand,
        }
        
        private Modes mode = Modes.OnDemand;
        private float timeLastSensed;

        public void SetUpdateMode(Modes newMode)
        {
            mode = newMode;
        }
        
        public void SetResourceType(ResourceTypes type)
        {
            resourceType = type;
        }

        public ResourceBase[] ScanResources(ResourceTypes type)
        {
            var mask = 1 << LayerMask.NameToLayer("Resource");
            var colliders = Physics.OverlapSphere(agent.Transform.position, DetectionRadius, mask);
            
            agent.LogInfo(this, $"found {colliders.Length} {type} resources");

            return colliders.Select(c => c.GetComponentInParent<ResourceBase>()).Where(r => r.resourceType == type).ToArray();
        }

        public ResourceBase GetClosestResource(ResourceTypes type, Vector3 pos)
        {
            var resources = ScanResources(type);

            float dist = float.MaxValue;
            ResourceBase closestResource = null;
            foreach (var resource in resources)
            {
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
            if (mode != Modes.PeriodicUpdate || resourceType == ResourceTypes.None) return;
            
            if (Time.time > timeLastSensed + RefreshRate)
            {
                timeLastSensed = Time.time;
                var resources = ScanResources(resourceType);
                // TODO set in memory?
            }
        }

        protected override void OnRemove()
        {
        }

        protected override void OnEnable()
        {
            // Once enabled, run the cooldown time before searching again
            timeLastSensed = Time.time;
        }
    }
}