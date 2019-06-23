using Aspekt.AI;
using Aspekt.Items;
using UnityEngine;

namespace Aspekt.Drones
{
    public abstract class ActionModule : InventoryItem, IDroneModule
    {
        private DroneAction action;
        
        public void AttachTo(DroneAIAgent agent)
        {
            agent.Actions.AddAction(action);
            action.OnAttach(agent.Owner.GetComponent<Drone>());
        }

        public void RemoveFrom(DroneAIAgent agent)
        {
            agent.Actions.AddAction(action);
        }

        protected abstract DroneAction GetAction();
        
        private void Awake()
        {
            action = GetAction();
        }
    }
}