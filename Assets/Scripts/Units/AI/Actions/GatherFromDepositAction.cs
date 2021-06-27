using System;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Move to an item (stored in memory) and pick it up
    /// </summary>
    [Serializable]
    public class GatherFromDepositAction : DroneAction
    {
        public float gatherDistance = 2f;

        private IStateMachine<AIAttributes, object> stateMachine;
        
        // TODO setup as animation
        private bool isGathering;
        private float gatherPercent;

        private IMovement movement;
        private IGatherer gatherer;
        
        public override float Cost => 5f;
        
        public override bool CheckComponents()
        {
            if (movement == null || gatherer == null)
            {
                movement = GetAbility<IMovement>();
                gatherer = GetAbility<IGatherer>();
            }
            
            return movement != null && gatherer != null;
        }

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            var deposit = (GatherableDeposit)Agent.Memory.Get(AIAttributes.DepositToGather);
            if (deposit == null) return false;

            this.stateMachine = stateMachine;

            var moveState = new MoveState(Agent, movement);
            var gatherState = new GatherState(Agent, gatherer);
            moveState.SetTarget(deposit.transform, gatherDistance);
            gatherState.SetTarget(deposit);
            
            stateMachine.Enqueue(moveState);
            stateMachine.Enqueue(gatherState);
            stateMachine.OnComplete += OnComplete;
            
            return true;
        }

        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.HasDepositToGather, true);
        }
        
        protected override void SetEffects()
        {
            AddEffect(AIAttributes.IsHoldingItem, true);
        }

        protected override bool CheckProceduralConditions()
        {
            if (Agent.Memory.Get(AIAttributes.DepositToGather) == null) return false;
            if (Agent.Memory.IsTrue(AIAttributes.IsHoldingItem)) return false;
            if (Agent.Memory.IsMatch(AIAttributes.HasDepositToGather, false)) return false;
            return true;
        }

        protected override void OnTick(float deltaTime)
        {
        }

        private void OnComplete()
        {
            stateMachine.OnComplete -= OnComplete;
            Agent.Memory.Set(AIAttributes.DepositToGather, null);
            Agent.Memory.Set(AIAttributes.HasDepositToGather, false);
            ActionSuccess();
        }
    }
}