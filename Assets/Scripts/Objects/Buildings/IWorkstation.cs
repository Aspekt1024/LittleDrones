
using System.Collections.Generic;

namespace Aspekt.Drones
{
    /// <summary>
    /// Can be used by an IWorker
    /// </summary>
    public interface IWorkstation
    {
        List<IWorker> Workers { get; }
        
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