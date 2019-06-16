using UnityEngine;

namespace Aspekt.Drones
{
    public class ResourceBase : MonoBehaviour, IGrabbableItem
    {
        public ResourceTypes resourceType;
        
        public Transform Transform => transform;

        public override string ToString()
        {
            return resourceType.ToString();
        }
    }
}