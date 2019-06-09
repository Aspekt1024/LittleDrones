using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// An item that exists in the Drone world
    /// </summary>
    public interface IItem
    {
        Transform Transform { get; }
    }
}