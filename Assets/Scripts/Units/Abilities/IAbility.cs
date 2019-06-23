using UnityEngine;

namespace Aspekt.Drones
{
    public interface IAbility
    {
        /// <summary>
        /// Determines if the ability is enabled and can be used
        /// </summary>
        bool Enabled { get; set; }
    }
}