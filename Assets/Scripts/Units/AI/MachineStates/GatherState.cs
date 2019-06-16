using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class GatherState : MachineState<AIAttributes, object>
    {
        private readonly IGatherer gatherer;
        
        private IGatherable gatherable;
        private float gatherPercent;


        public GatherState(IAIAgent<AIAttributes, object> agent, IGatherer gatherer) : base(agent)
        {
            this.gatherer = gatherer;
        }
        
        public void SetTarget(IGatherable target)
        {
            gatherable = target;
        }
        
        public override void Start()
        {
            var success = gatherer.StartGathering(gatherable, OnGatherComplete);
            if (!success)
            {
                StateComplete();
            }
        }

        public override void Pause()
        {
            gatherable.RemoveGatherer(gatherer);
        }

        public override void Stop()
        {
            gatherable.RemoveGatherer(gatherer);
        }

        public override void Tick(float deltaTime)
        {
            gatherer.Tick(deltaTime);
        }

        private void OnGatherComplete(IGatherable g)
        {
            Agent.Memory.Set(AIAttributes.HeldItem, g.GetItem());
            StateComplete();
        }
       
    }
}