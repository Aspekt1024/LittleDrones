using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class MoveState : MachineState<AIAttributes, object>
    {
        private readonly IMovement movement;
        
        private Transform targetTf;
        private Vector3 targetPos;

        private float targetReachedDistance;

        public MoveState(IAIAgent<AIAttributes, object> agent, IMovement movement) : base(agent)
        {
            this.movement = movement;
        }

        public override void Start()
        {
            movement.Run();
            if (targetTf != null)
            {
                movement.MoveTo(targetTf);
            }
            else
            {
                movement.MoveTo(targetPos);
            }
        }

        public override void Pause()
        {
            movement.Stop();
        }

        public override void Stop()
        {
            movement.Stop();
            StateComplete();
        }

        public override void Tick(float deltaTime)
        {
            movement.Tick(deltaTime);
            if (Vector3.Distance(Agent.Owner.transform.position, targetTf.position) < targetReachedDistance)
            {
                Stop();
            }
        }

        public void SetTarget(Vector3 position, float distance)
        {
            targetReachedDistance = distance;
            targetTf = null;
            targetPos = position;
        }

        public void SetTarget(Transform target, float distance)
        {
            targetReachedDistance = distance;
            targetTf = target;
        }
    }
}