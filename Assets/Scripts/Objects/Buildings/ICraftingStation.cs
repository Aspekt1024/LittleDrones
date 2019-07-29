using System.Collections.Generic;

namespace Aspekt.Drones
{
    public interface ICraftingStation
    {
        List<IGrabbableItem> Materials { get; }
        
        /// <summary>
        /// Adds a material to the crafting station
        /// </summary>
        /// <param name="item">the item to be added as a material</param>
        /// <returns>true if the item can be added as a material</returns>
        bool AddMaterial(IGrabbableItem item);

        /// <summary>
        /// Removes a material from the crafting station. See <see cref="Materials"/> for a
        /// list of materials that can be removed.
        /// </summary>
        /// <param name="item">the material to be removed.</param>
        /// <returns>true if the item can be removed</returns>
        bool RemoveMaterial(IGrabbableItem item);
        
        /// <summary>
        /// Adds a worker to begin crafting. Returns true if crafting can commence
        /// </summary>
        bool AddWorker(IWorker crafter);

        /// <summary>
        /// Removes a worker from the station
        /// </summary>
        /// <param name="crafter"></param>
        /// <returns></returns>
        void RemoveWorker(IWorker crafter);
    }
}