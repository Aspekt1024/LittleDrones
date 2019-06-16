using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Move to an item (stored in memory) and pick it up
    /// </summary>
    [Serializable]
    public class PickupItemAction : AIAction<AIAttributes, object>
    {
        public float grabDistance = 2f;
        public float gatherTime = 0.5f;

        private MoveState moveState;
        
        // TODO setup as animation
        private bool isGathering;
        private float timeStartedGathering;

        private IGrabbableItem item;
        
        public override float Cost => 1f; // TODO update to return the distance to the closest resource

        public override bool CheckComponents()
        {
            // TODO check can move
            // TODO check can pick up items (has grabber)
            return true;
        }

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            item = (IGrabbableItem)Agent.Memory.Get(AIAttributes.ItemToGather);
            if (item == null) return false;

            isGathering = false;
            
            moveState = new MoveState(Agent, Agent.Owner.GetComponent<IMoveable>().GetMovement());
            moveState.SetTarget(item.Transform, grabDistance);
            stateMachine.Enqueue(moveState);
            stateMachine.OnComplete += OnTargetReached;
            
            return true;
        }

        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.HasItemToGather, true);
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.IsHoldingItem, true);
            AddEffect(AIAttributes.HasItemToGather, false);
        }

        protected override bool CheckProceduralConditions()
        {
            return true;
        }

        protected override void OnTick(float deltaTime)
        {
            if (!isGathering || Time.time < timeStartedGathering + gatherTime) return;
            
            isGathering = false;
            Agent.Memory.Remove(AIAttributes.ItemToGather);
            Agent.Memory.Set(AIAttributes.HeldItem, item); // TODO delegate to grab ability
            item.Transform.gameObject.SetActive(false);
            ActionSuccess();
        }

        private void OnTargetReached()
        {
            if (isGathering) return;
            
            isGathering = true;
            timeStartedGathering = Time.time;
        }
    }
}