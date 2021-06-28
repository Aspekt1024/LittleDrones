using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class MoveState : MachineState<AIAttributes, object>, IMovement.IObserver
    {
        private readonly IMovement movement;
        private readonly Transform targetTf;
        private readonly float targetReachedDistance;

        public MoveState(IAIAgent<AIAttributes, object> agent, IMovement movement, Transform targetTf, float targetReachedDistance = -1f) : base(agent)
        {
            this.movement = movement;
            this.targetTf = targetTf;
            
            if (targetReachedDistance > 0)
            {
                this.targetReachedDistance = targetReachedDistance;
            }
            else
            {
                var colliders = targetTf.GetComponents<Collider>().Where(c => !c.isTrigger).ToArray();
                if (!colliders.Any())
                {
                    this.targetReachedDistance = 3f;
                }
                else
                {
                    this.targetReachedDistance = colliders[0].bounds.extents.x + 3f;
                }
            }
        }

        public override void Start()
        {
            movement.RegisterObserver(this);
            movement.Run();
            if (targetTf != null)
            {
                movement.MoveTo(targetTf, targetReachedDistance);
            }
        }

        public override void Pause()
        {
            movement.Stop();
        }

        public override void Stop()
        {
            movement.UnregisterObserver(this);
            movement.Stop();
        }

        public override void Tick(float deltaTime)
        {
        }

        public void OnTargetReached()
        {
            movement.UnregisterObserver(this);
            StateComplete();
        }
    }
}