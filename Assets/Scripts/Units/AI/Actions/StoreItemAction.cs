using System;
using System.Collections.Generic;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Move to a storage location and deposit the held item
    /// </summary>
    [Serializable]
    public class StoreItemAction : AIAction<AIAttributes, object>
    {
        public float placementDistance = 2f;
        
        public override float Cost => 1f; // TODO update to return the distance to the closest storage 
        
        private IGrabbableItem item;
        private MoveState moveState;

        // TODO building and storage are different interfaces of the same object
        private BuildingBase building;
        private IStorage storage;

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            item = (IGrabbableItem)Agent.Memory.Get(AIAttributes.HeldItem);
            if (item == null) return false;

            var type = BuildingTypes.None;
            if (item is ResourceBase)
            {
                type = BuildingTypes.ResourceDepot;
            }
            if (type == BuildingTypes.None) return false;

            var buildingSensor = Agent.Sensors.Get<BuildingSensor>();
            if (buildingSensor == null) return false;

            building = buildingSensor.FindClosestBuilding(type);
            if (building == null || !(building is IStorage s)) return false;
            storage = s;

            moveState = stateMachine.AddState<MoveState>();
            moveState.MoveTo(building.Transform);
            
            return true;
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.HasGatheredResource, true);
            AddEffect(AIAttributes.IsHoldingItem, false);
        }
        
        protected override void OnTick(float deltaTime)
        {
            if (item == null || storage == null)
            {
                ActionFailure();
                return;
            }
            
            var dist = Vector3.Distance(building.Transform.position, Agent.Owner.transform.position);
            if (dist > placementDistance) return;

            Agent.Memory.Remove(AIAttributes.HeldItem);
            bool success = storage.TakeItem(item);
            if (success)
            {
                ActionSuccess();
            }
            else
            {
                ActionFailure();
            }
        }

        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.CanMove, true);
            AddPrecondition(AIAttributes.CanPickupItems, true);
            AddPrecondition(AIAttributes.HasBuildingSensor, true);
            AddPrecondition(AIAttributes.IsHoldingItem, true);
        }
    }
}