using System;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Move to an item (stored in memory) and pick it up
    /// </summary>
    [Serializable]
    public class GatherFromDepositAction : AIAction<AIAttributes, object>
    {
        public float gatherDistance = 2f;

        private IStateMachine<AIAttributes, object> stateMachine;
        
        // TODO setup as animation
        private bool isGathering;
        private float gatherPercent;
        public override float Cost => 5f; // TODO update to return the distance to the closest deposit plus time to gather

        public override bool CheckComponents()
        {
            // TODO check can move
            // TODO check can gather (has gatherer component)
            return true;
        }

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            var deposit = (GatherableDeposit)Agent.Memory.Get(AIAttributes.DepositToGather);
            if (deposit == null) return false;

            this.stateMachine = stateMachine;

            var moveState = new MoveState(Agent, Agent.Owner.GetComponent<IMoveable>().GetMovement());
            var gatherState = new GatherState(Agent, Agent.Owner.GetComponent<ICanGather>().GetGatherer());
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
            AddEffect(AIAttributes.DepositToGather, null);
            AddEffect(AIAttributes.HasDepositToGather, false);
            AddEffect(AIAttributes.IsHoldingItem, true);
        }

        protected override bool CheckProceduralConditions()
        {
            if (Agent.Memory.Get(AIAttributes.DepositToGather) == null) return false;
            if (Agent.Memory.Get(AIAttributes.IsHoldingItem) != null) return false;
            if (Agent.Memory.IsMatch(AIAttributes.HasDepositToGather, false)) return false;
            return true;
        }

        protected override void OnTick(float deltaTime)
        {
        }

        private void OnComplete()
        {
            stateMachine.OnComplete -= OnComplete;
            ActionSuccess();
        }
    }
}