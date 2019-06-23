using System;
using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Finds the closest item of the given type
    /// </summary>
    [Serializable]
    public class FindResourceAction : DroneAction
    {
        public float scanTime = 0.5f;

        private ResourceSensor sensor;
        private ResourceTypes resourceType;
        
        public override float Cost => 1f;
        
        public override bool CheckComponents()
        {
            resourceType = (ResourceTypes)Agent.Memory.Get(AIAttributes.ResourceGoalType);
            if (resourceType == ResourceTypes.None) return false;

            sensor = Agent.Sensors.Get<ResourceSensor>();
            return sensor != null && sensor.ScanResources(resourceType).Any();
        }

        // TODO set as animation
        private float timeStartedScanning;
        
        protected override void SetPreconditions()
        {
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.HasItemToGather, true);
        }

        protected override bool CheckProceduralConditions()
        {
            return true;
        }

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            sensor = Agent.Sensors.Get<ResourceSensor>();
            if (sensor == null) return false;

            resourceType = (ResourceTypes)Agent.Memory.Get(AIAttributes.ResourceGoalType);
            if (resourceType == ResourceTypes.None) return false;

            timeStartedScanning = Time.time;
            
            return true;
        }

        protected override void OnTick(float deltaTime)
        {
            if (Time.time < timeStartedScanning + scanTime) return;
            
            var item = sensor.GetClosestResource(resourceType, Agent.Transform.position);
            if (item == null)
            {
                ActionFailure();
            }
            else
            {
                Agent.Memory.Set(AIAttributes.ItemToGather, item);
                ActionSuccess();
            }
        }
    }
}