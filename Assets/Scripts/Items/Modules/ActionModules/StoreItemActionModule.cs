using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Store Item Action", menuName = "Drone/Action Module/Store Item")]
    public class StoreItemActionModule : ActionModule
    {
        public StoreItemAction pickupAction = new StoreItemAction();

        protected override AIAction<AIAttributes, object> CreateAction()
        {
            return pickupAction;
        }
    }
}