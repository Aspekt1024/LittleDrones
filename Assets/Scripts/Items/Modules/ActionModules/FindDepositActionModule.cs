using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Find Deposit Action", menuName = "Drone/Action Module/Find Deposit")]
    public class FindDepositActionModule : ActionModule
    {
        public FindDepositAction action = new FindDepositAction();

        protected override AIAction<AIAttributes, object> CreateAction()
        {
            return action;
        }
    }
}