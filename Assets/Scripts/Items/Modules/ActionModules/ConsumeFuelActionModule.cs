using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Consume Fuel Action", menuName = "Drone/Action Module/Consume Fuel")]
    public class ConsumeFuelActionModule : ActionModule
    {
        private readonly ConsumeFuelAction action = new ConsumeFuelAction();
        
        protected override DroneAction GetAction()
        {
            return action;
        }
    }
}