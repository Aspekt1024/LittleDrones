using System;
using UnityEngine;

namespace Aspekt.Drones
{
    public class GatherComponent : IGatherer
    {
        private IGatherable gatherable;
        private float gatherPercent;
        private Action<IGatherable> gatheringCompletedCallback;

        private Unit unit;
        
        public GatherComponent(Unit unit)
        {
            this.unit = unit;
        }

        private enum States
        {
            None, Gathering
        }

        private States state = States.None;
        
        public IGrabbableItem HeldItem { get; private set; }

        public bool Enabled { get; set; } = true;
        
        public bool HoldItem(IGrabbableItem item)
        {
            if (HeldItem != null) return false;
            HeldItem = item;
            return true;
        }

        public IGrabbableItem ReleaseItem()
        {
            var item = HeldItem;
            HeldItem = null;
            return item;
        }

        public bool StartGathering(IGatherable target, Action<IGatherable> completionCallback)
        {
            var success = target.AddGatherer(this);
            if (!success) return false;
            
            state = States.Gathering;
            gatherPercent = 0f;
            gatherable = target;
            gatheringCompletedCallback = completionCallback;
            return true;
        }

        public void StopGathering()
        {
            state = States.None;
            gatherable.RemoveGatherer(this);
            GameManager.UI.Get<ProgressUI>().StopProgress(unit.transform);
        }

        public void Tick(float deltaTime)
        {
            if (state != States.Gathering) return;
            
            gatherPercent = gatherable.IncrementGatherPercent(gatherPercent, 0, deltaTime);
            if (gatherPercent > 1f)
            {
                StopGathering();
                Hold(gatherable.GetItem());
                gatheringCompletedCallback?.Invoke(gatherable);
                GameManager.UI.Get<ProgressUI>().ShowProgressComplete(unit.transform);
            }
            else
            {
                GameManager.UI.Get<ProgressUI>().ShowProgress(gatherPercent, unit.transform);
            }
        }

        private void Hold(IGrabbableItem item)
        {
            HeldItem = item;
            item.Transform.gameObject.SetActive(false);
        }
    }
}