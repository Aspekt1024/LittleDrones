using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New  Gather From Deposit Action", menuName = "Drone/Action Module/Gather From Deposit")]
    public class GatherFromDepositActionModule : ActionModule
    {
        public GatherFromDepositAction action = new GatherFromDepositAction();

        protected override DroneAction GetAction()
        {
            return action;
        }
    }
}