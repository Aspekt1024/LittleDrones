using Aspekt.AI;
using Aspekt.Items;
using UnityEngine;

namespace Aspekt.Drones
{
    public abstract class ActionModule : InventoryItem, IDroneModule
    {
        private IAIAction<AIAttributes, object> action;
        
        public void AttachTo(DroneAIAgent agent)
        {
            agent.Actions.AddAction(action);
        }

        public void RemoveFrom(DroneAIAgent agent)
        {
            agent.Actions.AddAction(action);
        }

        public virtual bool IsTypeMatch(ActionModule other)
        {
            return other.GetType() == GetType();
        }
        
        private void Awake()
        {
            action = CreateAction();
        }

        protected abstract AIAction<AIAttributes, object> CreateAction();
    }
}