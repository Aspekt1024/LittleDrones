using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Finds the closest item of the given type
    /// </summary>
    public class FindResourceAction : AIAction<AIAttributes, object>
    {
        public override float Cost => 1f;
        public float ScanTime = 0.5f;
        
        private ResourceSensor sensor;
        private ResourceTypes resourceType;

        // TODO set as animation
        private float timeStartedScanning;
        
        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.HasResourceSensor, true);
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.HasItemToGather, true);
        }

        protected override void OnTick(float deltaTime)
        {
            if (Time.time < timeStartedScanning + ScanTime) return;
            
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

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            sensor = Agent.Sensors.Get<ResourceSensor>();
            if (sensor == null) return false;

            resourceType = (ResourceTypes)Agent.Memory.Get(AIAttributes.ResourceGoalType);
            if (resourceType == ResourceTypes.None) return false;

            timeStartedScanning = Time.time;
            
            return true;
        }
    }
}