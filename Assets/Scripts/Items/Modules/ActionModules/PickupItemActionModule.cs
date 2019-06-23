using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Pickup Item Action", menuName = "Drone/Action Module/Pickup Item")]
    public class PickupItemActionModule : ActionModule
    {
        public PickupItemAction action = new PickupItemAction();

        protected override DroneAction GetAction()
        {
            return action;
        }
    }
}