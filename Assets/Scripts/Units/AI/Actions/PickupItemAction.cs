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
        public float GrabDistance = 2f;
        public float GatherTime = 0.5f;

        // TODO setup as animation
        private bool isGathering;
        private float timeStartedGathering;
        
        public override float Cost => 1f; // TODO update to return the distance to the closest resource 
        
        private IGrabbableItem item;
        private MoveState moveState;

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            item = (IGrabbableItem)Agent.Memory.Get(AIAttributes.ItemToGather);
            if (item == null) return false;

            moveState = stateMachine.AddState<MoveState>();
            moveState.MoveTo(item.Transform);
            
            return true;
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.IsHoldingItem, true);
            AddEffect(AIAttributes.HasItemToGather, false);
        }
        
        protected override void OnTick(float deltaTime)
        {
            if (item == null)
            {
                ActionFailure();
                return;
            }
            
            var dist = Vector3.Distance(item.Transform.position, Agent.Owner.transform.position);
            if (dist > GrabDistance) return;

            if (!isGathering)
            {
                moveState.Stop();
                isGathering = true;
                timeStartedGathering = Time.time;
            }

            if (Time.time > timeStartedGathering + GatherTime)
            {
                isGathering = false;
                Agent.Memory.Remove(AIAttributes.ItemToGather);
                Agent.Memory.Set(AIAttributes.HeldItem, item); // TODO delegate to grab ability
                item.Transform.gameObject.SetActive(false);
                ActionSuccess();
            }
        }

        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.CanMove, true);
            AddPrecondition(AIAttributes.CanPickupItems, true);
            AddPrecondition(AIAttributes.HasItemToGather, true);
        }
    }
}