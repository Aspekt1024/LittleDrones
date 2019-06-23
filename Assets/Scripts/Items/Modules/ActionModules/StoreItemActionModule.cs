using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Store Item Action", menuName = "Drone/Action Module/Store Item")]
    public class StoreItemActionModule : ActionModule
    {
        public StoreItemAction action = new StoreItemAction();

        protected override DroneAction GetAction()
        {
            return action;
        }
    }
}