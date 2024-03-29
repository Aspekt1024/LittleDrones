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
    public class StoreItemAction : DroneAction
    {
        public float placementDistance = 2f;

        private IGrabbableItem item;
        private IMovement movement;

        // TODO building and storage are different interfaces of the same object
        private BuildingBase building;
        private IStorage storage;
        
        public override float Cost => 1f; // TODO update to return the distance to the closest storage 
        

        public override bool CheckComponents()
        {
            if (movement == null)
            {
                movement = GetAbility<IMovement>();
            }
            return movement != null;
        }

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

            building = buildingSensor.FindClosestBuilding(type, Agent.Owner.transform.position);
            if (building == null || !(building is IStorage s)) return false;
            storage = s;
            
            var target = building.pathingPoint == null ? building.Transform : building.pathingPoint;
            var moveState = new MoveState(Agent, movement, target);
            stateMachine.Enqueue(moveState);
            stateMachine.OnComplete += OnTargetReached;
            
            return true;
        }

        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.IsHoldingItem, true);
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.GatherResourceGoal, true);
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

        private void OnTargetReached()
        {
            bool success = storage.TakeItem(item);
            if (success)
            {
                ActionSuccess();
                Agent.Memory.Remove(AIAttributes.HeldItem);
                Agent.Memory.Set(AIAttributes.IsHoldingItem, false);
            }
            else
            {
                ActionFailure();
            }
        }
    }
}