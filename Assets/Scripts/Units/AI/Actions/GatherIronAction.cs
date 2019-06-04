using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class GatherIronAction : AIAction<AIAttributes, object>
    {
        private const ResourceTypes ResourceType = ResourceTypes.Iron;

        private ResourceBase resource;
        
        public override bool Begin()
        {
            var sensor = agent.GetSensor<ResourceSensor>();
            if (sensor == null)
            {
                Debug.Log("sensor doesn't exist");
                return false;
            }
            
            var ironSources = sensor.ScanResources(ResourceType);
            if (!ironSources.Any())
            {
                Debug.Log("no resources found");
                return false;
            }
            
            float distance = float.MaxValue;
            foreach (var iron in ironSources)
            {
                float dist = Vector3.Distance(agent.GetTransform().position, iron.transform.position);
                if (dist < distance)
                {
                    distance = dist;
                    resource = iron;
                }
            }
            return true;
        }

        public override bool IsComplete()
        {
            if (resource == null) return false;
            return Vector3.Distance(agent.GetTransform().position, resource.transform.position) < 1f;
        }

        protected override void OnTick(float deltaTime)
        {
            if (resource == null) return;
            Debug.Log("tracking resource: " + resource.name);
        }

        protected override void OnRemove()
        {
            Debug.Log("removing gather resource action");
        }
    }
}