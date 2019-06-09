using UnityEngine;

namespace Aspekt.Drones
{
    public enum ResourceTypes
    {
        None, Iron, Coal, Copper
    }
    
    public abstract class ResourceBase : MonoBehaviour, IItem
    {
        public ResourceTypes resourceType;
        
        public Transform Transform => transform;
    }
}