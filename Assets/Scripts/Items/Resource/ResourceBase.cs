using UnityEngine;

namespace Aspekt.Drones
{
    public enum ResourceTypes
    {
        None, Iron, Coal, Copper
    }
    
    public abstract class ResourceBase : MonoBehaviour, IGrabbableItem
    {
        public ResourceTypes resourceType;
        
        public Transform Transform => transform;

        public override string ToString()
        {
            return resourceType.ToString();
        }
    }
}