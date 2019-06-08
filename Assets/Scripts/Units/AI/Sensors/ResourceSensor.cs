using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{

    public class ResourceSensor : Sensor<AIAttributes, object>
    {
        private const float DetectionRadius = 100f;
        private const float RefreshRate = 1f;
        private const bool RequireLineOfSight = false;

        private ResourceTypes resourceType = ResourceTypes.None;
        
        public void SetResourceType(ResourceTypes type)
        {
            resourceType = type;
        }
        
        public enum Modes
        {
            PeriodicUpdate, OnDemand,
        }
        
        private Modes mode = Modes.OnDemand;
        private float timeLastSensed;

        public void SetUpdateMode(Modes newMode)
        {
            mode = newMode;
            if (mode == Modes.PeriodicUpdate && resourceType == ResourceTypes.None)
            {
                Debug.LogError("periodic update set, but no resource type to scan");
            }
        }

        public ResourceBase[] ScanResources(ResourceTypes type)
        {
            var mask = 1 << LayerMask.NameToLayer("Resource");
            var colliders = Physics.OverlapSphere(agent.Transform.position, DetectionRadius, mask);
            
            Debug.Log("resource collider count: " + colliders.Length);
            // TODO line of sight

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
            if (mode != Modes.PeriodicUpdate) return;
            
            if (Time.time > timeLastSensed + RefreshRate)
            {
                timeLastSensed = Time.time;
                var resources = ScanResources(resourceType);
                // TODO set in memory?
            }
        }

        protected override void OnRemove()
        {
            // TODO remove objects from memory
        }

        protected override void OnEnable()
        {
            // Once enabled, run the cooldown time before searching again
            timeLastSensed = Time.time;
        }
    }
}