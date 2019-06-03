using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class GatherResourceAction : AIAction<AIAttributes, object>
    {
        protected override void OnTick(float deltaTime)
        {
            Debug.Log("require searching of attributes");
        }

        protected override void OnRemove()
        {
            Debug.Log("removing gather resource action");
        }
    }
}