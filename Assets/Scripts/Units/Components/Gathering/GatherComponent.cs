using System;

namespace Aspekt.Drones
{
    public class GatherComponent : IGatherer
    {
        private IGatherable gatherable;
        private float gatherPercent;
        private Action<IGatherable> gatheringCompletedCallback;

        private ICanGather parent;

        private enum States
        {
            None, Gathering
        }

        private States state = States.None;
        
        public IGrabbableItem HeldItem { get; private set; }
        
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

        public GatherComponent(ICanGather parent)
        {
            this.parent = parent;
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
            }
        }

        private void Hold(IGrabbableItem item)
        {
            HeldItem = item;
            item.Transform.gameObject.SetActive(false);
        }
    }
}