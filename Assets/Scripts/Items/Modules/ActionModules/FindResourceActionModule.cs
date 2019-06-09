using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Find Resource Action", menuName = "Drone/Action Module/Find Resource")]
    public class FindResourceActionModule : ActionModule
    {
        public FindResourceAction retrieveAction = new FindResourceAction();

        protected override AIAction<AIAttributes, object> CreateAction()
        {
            return retrieveAction;
        }
    }
}