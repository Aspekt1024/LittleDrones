using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Pickup Item Action", menuName = "Drone/Action Module/Pickup Item")]
    public class PickupItemActionModule : ActionModule
    {
        public PickupItemAction pickupAction = new PickupItemAction();

        protected override AIAction<AIAttributes, object> CreateAction()
        {
            return pickupAction;
        }
    }
}