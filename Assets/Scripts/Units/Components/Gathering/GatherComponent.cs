using System;

namespace Aspekt.Drones.Gathering
{
    public class GatherComponent : IGatherer
    {
        private IGatherable gatherable;
        private float gatherPercent;
        private Action<IGatherable> gatheringCompletedCallback;

        private enum States
        {
            None, Gathering
        }

        private States state = States.None;
        
        public bool StartGathering(IGatherable target, Action<IGatherable> completionCallback)
        {
            var success = target.AddGatherer(this);
            if (!success) return false;
            
            state = States.Gathering;
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
            }
        }
    }
}