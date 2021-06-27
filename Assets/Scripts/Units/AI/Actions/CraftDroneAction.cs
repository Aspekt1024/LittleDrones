using System;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Move to a crafting station that has the required materials and craft a drone
    /// </summary>
    [Serializable]
    public class CraftDroneAction : DroneAction
    {
        [SerializeField] private float placementDistance = 2f;
        
        private IMovement movement;
        private IWorker worker;
        
        public override float Cost { get; } = 1f;
        
        private CraftingPad pad;
        private BuildingBase building;
        
        public override bool CheckComponents()
        {
            if (movement == null)
            {
                movement = GetAbility<IMovement>();
            }

            if (worker == null)
            {
                worker = GetAbility<IWorker>();
            }

            return movement != null && worker != null;
        }

        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.IsHoldingItem, false);
            AddPrecondition(AIAttributes.CraftingStationReady, true);
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.CraftDroneGoal, true);
        }

        protected override bool CheckProceduralConditions()
        {
            return true;
        }

        protected override void OnTick(float deltaTime)
        {
            if (pad == null || !(pad.HasResources && pad.CanWorkHere(worker)))
            {
                ActionFailure();
            }
        }

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            pad = (CraftingPad) Agent.Memory.Get(AIAttributes.TargetBuilding);
            if (pad == null || !(pad is BuildingBase b)) return false;
            building = b;
            
            var moveState = new MoveState(Agent, movement);
            moveState.SetTarget(building.Transform, placementDistance);
            stateMachine.Enqueue(moveState);
            stateMachine.OnComplete += OnTargetReached;

            return true;
        }
        
        private void OnTargetReached()
        {
            Agent.Memory.Remove(AIAttributes.TargetBuilding);
            bool success = pad.AddWorker(worker);
            if (success)
            {
                ActionSuccess();
            }
            else
            {
                ActionFailure();
            }
        }
    }
}