using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// A crafting station for making drones
    /// </summary>
    public class CraftingPad : BuildingBase, ICraftingStation
    {
        [SerializeField] private int numIronRequired;
        [SerializeField] private int numCopperRequired;
        [SerializeField] private int numCoalRequired;
        [SerializeField] private float timeToCraft;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private int maxWorkers = 1;

        private int numIron;
        private int numCopper;
        private int numCoal;
        private float craftTime;

        public List<IWorker> Workers { get; } = new List<IWorker>();
        
        public List<IGrabbableItem> Materials { get; } = new List<IGrabbableItem>();
        
        public bool NeedsIron => numIron < numIronRequired;
        public bool NeedsCopper => numCopper < numCopperRequired;
        public bool NeedsCoal => numCoal < numCoalRequired;
        
        public bool AddMaterial(IGrabbableItem item)
        {
            if (!(item is ResourceBase resource)) return false;

            switch (resource.resourceType)
            {
                case ResourceTypes.None:
                    return false;
                case ResourceTypes.Iron:
                    if (!NeedsIron) return false;
                    numIron++;
                    break;
                case ResourceTypes.Coal:
                    if (!NeedsCoal) return false;
                    numCoal++;
                    break;
                case ResourceTypes.Copper:
                    if (!NeedsCopper) return false;
                    numCopper++;
                    break;
                default:
                    Debug.LogError("invalid resource type: " + resource.resourceType);
                    return false;
            }
            
            Materials.Add(item);
            return true;
        }

        public bool RemoveMaterial(IGrabbableItem item)
        {
            if (!Materials.Contains(item)) return false;
            if (!(item is ResourceBase resource)) return false;

            switch (resource.resourceType)
            {
                case ResourceTypes.None:
                    return false;
                case ResourceTypes.Iron:
                    numIron--;
                    break;
                case ResourceTypes.Coal:
                    numCoal--;
                    break;
                case ResourceTypes.Copper:
                    numCopper--;
                    break;
                default:
                    Debug.LogError("invalid resource type: " + resource.resourceType);
                    return false;
            }

            Materials.Remove(item);
            return true;
        }

        /// <summary>
        /// Adds a worker to the crafting pad.
        /// Returns true if the station is available and a drone can be crafted.
        /// </summary>
        public bool AddWorker(IWorker crafter)
        {
            if (Workers.Count >= maxWorkers) return false;
            if (NeedsCoal || NeedsCopper || NeedsIron) return false;
            Workers.Add(crafter);
            return true;
        }

        public void RemoveWorker(IWorker crafter)
        {
            Workers.Remove(crafter);
        }

        private void Update()
        {
            if (!Workers.Any()) return;

            foreach (var worker in Workers)
            {
                float scale = 1f + worker.WorkerSkill / 4f;
                craftTime += Time.deltaTime * scale;
            }
            if (craftTime < timeToCraft) return;
            
            foreach (var worker in Workers)
            {
                worker.JobComplete();
            }
            Workers.Clear();
            GameManager.Units.CreateUnit<Drone>(spawnPoint.position);
        }
    }
}