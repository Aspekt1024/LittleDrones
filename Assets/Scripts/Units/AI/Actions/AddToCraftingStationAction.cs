using System;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Move to a crafting station and deposit the held item
    /// </summary>
    [Serializable]
    public class AddToCraftingStationAction : DroneAction
    {
        public float placementDistance = 2f;

        private IGrabbableItem item;
        private IMovement movement;

        // TODO building and station are different interfaces of the same object
        private BuildingBase building;
        private ICraftingStation station;
        
        public override float Cost { get; } = 1f; // TODO update to return the distance to the closest station
        public override bool CheckComponents()
        {
            if (movement == null)
            {
                movement = GetAbility<IMovement>();
            }
            
            return movement != null;
        }

        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.IsHoldingResource, true);
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.HasSuppliedCraftingStation, true);
            AddEffect(AIAttributes.IsHoldingResource, false);
        }

        protected override bool CheckProceduralConditions()
        {
            return true;
        }

        protected override void OnTick(float deltaTime)
        {
            if (item == null || building == null)
            {
                ActionFailure();
            }
        }

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            item = (IGrabbableItem)Agent.Memory.Get(AIAttributes.HeldItem);
            if (item == null) return false;

            if (!(item is ResourceBase resource)) return false;
            building = GetCraftingPad(resource);
            
            if (building == null || !(building is ICraftingStation s)) return false;
            station = s;

            var moveState = new MoveState(Agent, movement);
            moveState.SetTarget(building.Transform, placementDistance);
            stateMachine.Enqueue(moveState);
            stateMachine.OnComplete += OnTargetReached;
            
            return true;
        }

        private void OnTargetReached()
        {
            Agent.Memory.Remove(AIAttributes.HeldItem);
            bool success = station.AddMaterial(item);
            if (success)
            {
                ActionSuccess();
            }
            else
            {
                ActionFailure();
            }
        }

        private BuildingBase GetCraftingPad(ResourceBase resource)
        {
            var buildingSensor = Agent.Sensors.Get<BuildingSensor>();
            if (buildingSensor == null) return null;

            Predicate<CraftingPad> predicate;
            switch (resource.resourceType)
            {
                case ResourceTypes.None:
                    return null;
                case ResourceTypes.Iron:
                    predicate = c => c.NeedsIron;                    
                    break;
                case ResourceTypes.Coal:
                    predicate = c => c.NeedsCoal;
                    break;
                case ResourceTypes.Copper:
                    predicate = c => c.NeedsCopper;
                    break;
                default:
                    Debug.LogError("invalid resource type: " + resource.resourceType);
                    return null;
            }
            
            return buildingSensor.FindClosestBuilding(BuildingTypes.CraftingPad, Agent.Owner.transform.position, predicate);
        }
    }
}