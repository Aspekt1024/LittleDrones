using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Drones
{
    public abstract class GatherableDeposit : MonoBehaviour, IGatherable
    {
        public float timeToGather = 3f;
        public int maxGatherers = 1;
        public int initialResources = 500;
        public float skillFactor = 3f;
        
        private readonly List<IGatherer> gatherers = new List<IGatherer>();
        private int numResources;
        
        private void Awake()
        {
            numResources = initialResources;
        }
        
        public bool AddGatherer(IGatherer gatherer)
        {
            var numGatherers = gatherers.Count;
            if (numGatherers >= maxGatherers) return false;
            if (numResources < 1 + numGatherers) return false;
            gatherers.Add(gatherer);
            return true;
        }
    
        public void RemoveGatherer(IGatherer gatherer)
        {
            gatherers.Remove(gatherer);
        }

        public float IncrementGatherPercent(float currentPercent, float gatherSkill, float deltaTime)
        {
            return currentPercent + (1 + gatherSkill * skillFactor) * deltaTime / timeToGather;
        }

        public IGrabbableItem GetItem()
        {
            if (numResources < 1) return default;
            numResources--;
            return CreateGatherableItem();
        }

        public IGrabbableItem GetItemIgnoreAvailability()
        {
            return CreateGatherableItem();
        }

        protected abstract IGrabbableItem CreateGatherableItem();
    }
}