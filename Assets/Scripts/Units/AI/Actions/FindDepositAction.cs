using System;
using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class FindDepositAction : DroneAction
    {
        public float scanTime = 0.5f;

        private ObjectSensor objectSensor;
        private ResourceTypes resourceType;
        
        public override float Cost => 1f;

        public override bool CheckComponents()
        {
            resourceType = (ResourceTypes)Agent.Memory.Get(AIAttributes.ResourceGoalType);
            if (resourceType == ResourceTypes.None) return false;

            objectSensor = Agent.Sensors.Get<ObjectSensor>();
            bool foundDeposit = objectSensor.IsObjectObtainable<ResourceDeposit>(r => r.resourceType == resourceType);
            return objectSensor != null && foundDeposit;
        }

        // TODO set as animation
        private float timeStartedScanning;
        
        protected override void SetPreconditions()
        {
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.HasDepositToGather, true);
        }

        protected override bool CheckProceduralConditions()
        {
            return true;
        }

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            objectSensor = Agent.Sensors.Get<ObjectSensor>();
            if (objectSensor == null) return false;

            resourceType = (ResourceTypes)Agent.Memory.Get(AIAttributes.ResourceGoalType);
            if (resourceType == ResourceTypes.None) return false;

            timeStartedScanning = Time.time;
            
            return true;
        }

        protected override void OnTick(float deltaTime)
        {
            if (Time.time < timeStartedScanning + scanTime) return;
            
            var deposit = objectSensor.GetClosest<ResourceDeposit>(r => r.resourceType == resourceType);
            if (deposit == null)
            {
                ActionFailure();
            }
            else
            {
                Agent.Memory.Set(AIAttributes.DepositToGather, deposit);
                ActionSuccess();
            }
        }
    }
}