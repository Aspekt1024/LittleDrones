using System;
using System.Collections.Generic;
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
        public float GrabDistance = 1.2f;
        
        public override float Cost => 1f; // TODO update to return the distance to the closest resource 
        
        private IItem item;
        private MoveState moveState;

        protected override bool Begin(IStateMachine<AIAttributes, object> stateMachine)
        {
            item = (IItem)Agent.Memory.Get(AIAttributes.ItemToGather);
            if (item == null) return false;

            moveState = stateMachine.AddState<MoveState>();
            moveState.MoveTo(item.Transform);
            
            return true;
        }

        protected override void SetEffects()
        {
            AddEffect(AIAttributes.IsHoldingItem, true);
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
            
            Agent.Memory.Set(AIAttributes.HeldItem, item); // TODO delegate to grab ability
            ActionSuccess();
        }

        protected override void SetPreconditions()
        {
            AddPrecondition(AIAttributes.CanMove, true);
            AddPrecondition(AIAttributes.CanPickupItems, true);
            AddPrecondition(AIAttributes.HasItemToGather, true);
        }
    }
}