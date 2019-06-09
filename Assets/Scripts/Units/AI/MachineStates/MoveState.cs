using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class MoveState<L, V> : MachineState<L, V>
    {
        private IMovement movement;
        
        protected override void OnInit()
        {
            movement = agent.Owner.GetComponent<IMoveable>().GetMovement();
        }

        public override void Start()
        {
            movement.Run();
        }

        public override void Pause()
        {
            movement.Stop();
        }

        public override void Stop()
        {
            movement.Stop();
        }

        public override void Tick(float deltaTime)
        {
            movement.Tick(deltaTime);
        }

        public void MoveTo(Vector3 position) => movement.MoveTo(position, true);
        public void MoveTo(Transform tf) => movement.MoveTo(tf, true);
    }
}