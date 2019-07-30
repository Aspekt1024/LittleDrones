using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class ConsumeFuelAction: DroneAction
    {
        public override float Cost { get; } = 1f;

        private ResourceBase fuel;
        private DroneVitals vitals;
        
        public override bool CheckComponents()
        {
            if (vitals == null)
            {
                vitals = Agent.Owner.GetComponent<Drone>().Vitals;
            }
            return vitals != null;
        }

        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.IsHoldingResource, true);
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.HasLowFuel, false);
            AddEffect(AIAttributes.IsHoldingResource, false);
        }

        protected override bool CheckProceduralConditions()
        {
            return true;
        }

        protected override void OnTick(float deltaTime)
        {
            if (fuel == null)
            {
                ActionFailure();
            }
            
            Agent.Memory.Remove(AIAttributes.HeldItem);
            vitals.AddFuel(2f);
            Object.Destroy(fuel.gameObject);
            ActionSuccess();
        }

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            var item = (IGrabbableItem)Agent.Memory.Get(AIAttributes.HeldItem);
            if (!(item is ResourceBase resource) || resource.resourceType != ResourceTypes.Coal) return false;
            fuel = resource;
            return fuel != null;
        }
    }
}