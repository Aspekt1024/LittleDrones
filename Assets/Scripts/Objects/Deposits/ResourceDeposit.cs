using System;
using UnityEngine;

namespace Aspekt.Drones
{
    public class ResourceDeposit : GatherableDeposit
    {
        public ResourceTypes resourceType;

        private GameObject resourcePrefab;

        private void Start()
        {
            switch (resourceType)
            {
                case ResourceTypes.None:
                    break;
                case ResourceTypes.Iron:
                    resourcePrefab = Resources.Load<GameObject>("Items/Resources/Iron");
                    break;
                case ResourceTypes.Coal:
                    resourcePrefab = Resources.Load<GameObject>("Items/Resources/Coal");
                    break;
                case ResourceTypes.Copper:
                    resourcePrefab = Resources.Load<GameObject>("Items/Resources/Copper");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (resourcePrefab == null)
            {
                Debug.LogWarning($"{nameof(ResourceDeposit)} of type {resourceType} has a null prefab");
            }
        }
        
        public override string ToString()
        {
            return resourceType.ToString() + " Deposit";
        }

        protected override IGrabbableItem CreateGatherableItem()
        {
            return resourcePrefab == null ? null : Instantiate(resourcePrefab).GetComponent<ResourceBase>();
        }
    }
}