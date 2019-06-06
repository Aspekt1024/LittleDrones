using System;
using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class GatherIronAction : AIAction<AIAttributes, object>
    {
        private const ResourceTypes ResourceType = ResourceTypes.Iron;

        private ResourceBase resource;
        private IMovement movement; //TODO delegate to State in StateMachine
        
        public override bool Begin(IStateMachine<AIAttributes, object> stateMachine, Action onSuccessCallback, Action onFailureCallback)
        {
            var sensor = agent.Sensors.Get<ResourceSensor>();
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

            var moveable = agent.Owner.GetComponent<Drone>(); // TODO movement not handled here
            if (moveable == null)
            {
                Debug.Log("no movable component on agent");
                return false;
            }
            
            movement = moveable.GetMovement();
            if (movement == null)
            {
                Debug.Log("movable has no movement component");
                return false;
            }
            
            float distance = float.MaxValue;
            foreach (var iron in ironSources)
            {
                float dist = Vector3.Distance(agent.Transform.position, iron.transform.position);
                if (dist < distance)
                {
                    distance = dist;
                    resource = iron;
                }
            }
            
            movement.MoveTo(resource.transform, true);
            
            return true;
        }

        public override bool IsComplete()
        {
            if (resource == null) return false;
            return Vector3.Distance(agent.Transform.position, resource.transform.position) < 1f;
        }

        protected override void OnTick(float deltaTime)
        {
            if (resource == null) return;
            if (movement.TargetReached())
            {
                Debug.Log("reached target!");
            }
            
        }

        protected override void OnRemove()
        {
            Debug.Log("removing gather resource action");
        }
    }
}