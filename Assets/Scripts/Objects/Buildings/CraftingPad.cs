using System;
using System.Collections.Generic;
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

        private int numIron;
        private int numCopper;
        private int numCoal;
        private float craftTime;

        private IWorker currentWorker;
        
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
        /// Adds a worker to the crafting pad. Only one worker can be present at a time.
        /// Returns true if the station is free and a drone can be crafted.
        /// </summary>
        public bool AddWorker(IWorker crafter)
        {
            if (currentWorker != null) return false;
            if (NeedsCoal || NeedsCopper || NeedsIron) return false;
            currentWorker = crafter;
            return true;
        }

        public void RemoveWorker(IWorker crafter)
        {
            if (currentWorker == crafter)
            {
                currentWorker = null;
            }
        }

        private void Update()
        {
            if (currentWorker == null) return;
            
            float scale = 1f + currentWorker.WorkerSkill / 4f;
            craftTime += Time.deltaTime * scale;
            if (craftTime >= timeToCraft)
            {
                currentWorker.JobComplete();
                currentWorker = null;
                GameManager.Units.CreateUnit<Drone>(spawnPoint.position);
            }
        }
    }
}