using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// An item that can be picked up and dropped
    /// </summary>
    public interface IGrabbableItem
    {
        Transform Transform { get; }
    }
}