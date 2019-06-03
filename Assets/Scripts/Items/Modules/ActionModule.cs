using Aspekt.Items;
using UnityEngine;

namespace Aspekt.Drones
{
    public enum ActionModules
    {
        GatherResource
    }
    
    [CreateAssetMenu(fileName = "New Module", menuName = "Drone/Action Module")]
    public class ActionModule : InventoryItem, IDroneModule
    {
        public ActionModules actionType;
        
        public void AttachTo(DroneAIAgent agent)
        {
            switch (actionType)
            {
                case ActionModules.GatherResource:
                    agent.Actions.AddAction<GatherResourceAction>();
                    break;
                default:
                    Debug.LogError("invalid AI action type: " + actionType);
                    break;
            }
        }

        public void RemoveFrom(DroneAIAgent agent)
        {
            switch (actionType)
            {
                case ActionModules.GatherResource:
                    agent.Actions.RemoveAction<GatherResourceAction>();
                    break;
                default:
                    Debug.LogError("invalid AI action type: " + actionType);
                    break;
            }
        }
    }
}