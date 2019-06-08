using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class GatherIronAction : AIAction<AIAttributes, object>
    {
        private ResourceSensor sensor;
        private ResourceBase resource;

        private const ResourceTypes ResourceType = ResourceTypes.Iron;

        public override float Cost
        {
            get { return 1f; } // TODO update to return the distance to the closest resource 
        }

        public override bool Begin(IStateMachine<AIAttributes, object> stateMachine, Action onSuccessCallback, Action onFailureCallback)
        {
            sensor = agent.Sensors.Get<ResourceSensor>();
            if (sensor == null)
            {
                agent.Log(AILogType.Info, "sensor doesn't exist");
                return false;
            }
            
            resource = sensor.GetClosestResource(ResourceType, agent.Transform.position);
            if (resource == null)
            {
                agent.Log(AILogType.Info, "no resources found");
                return false;
            }
            
            return true;
        }

        public override Dictionary<AIAttributes, object> GetPreconditions()
        {
            var preconditions = new Dictionary<AIAttributes, object>();
            
            return preconditions;
        }

        public override bool CheckProceduralPreconditions()
        {
            if (!base.CheckProceduralPreconditions()) return false;
            if (resource == null)
            {
                resource = sensor.GetClosestResource(ResourceType, agent.Transform.position);
            }
            
            return resource != null;
        }

        public override Dictionary<AIAttributes, object> GetEffects()
        {
            var effects = new Dictionary<AIAttributes, object>();
            
            return effects;
        }

        public override bool IsComplete()
        {
            if (resource == null) return false;
            return Vector3.Distance(agent.Transform.position, resource.transform.position) < 1f;
        }

        protected override void OnTick(float deltaTime)
        {
            
        }

        protected override void OnRemove()
        {
            Debug.Log("removing gather resource action");
        }
    }
}