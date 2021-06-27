using System;
using UnityEngine;

namespace Aspekt.Drones
{
    public class ResourceDeposit : GatherableDeposit
    {
        public ResourceTypes resourceType;
        public ResourceBase resourcePrefab;
        public DepositAnimator.Settings animationSettings;

        private DepositAnimator depositAnim;
        
        private void Start()
        {
            if (resourcePrefab == null)
            {
                Debug.LogWarning($"{nameof(ResourceDeposit)} of type {resourceType} has a null prefab");
            }

            depositAnim = new DepositAnimator(animationSettings);
        }
        
        public override string ToString()
        {
            return resourceType + " Deposit";
        }

        protected override IGrabbableItem CreateGatherableItem()
        {
            return resourcePrefab == null ? null : Instantiate(resourcePrefab);
        }

        private void Update()
        {
            depositAnim.Tick();
        }
    }
}