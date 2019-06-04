using UnityEngine;

namespace Aspekt.Drones
{
    public enum ResourceTypes
    {
        None, Iron, Coal, Copper
    }
    
    public abstract class ResourceBase : MonoBehaviour
    {
        public ResourceTypes resourceType;
    }
}