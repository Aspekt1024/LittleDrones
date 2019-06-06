using Aspekt.AI;

namespace Aspekt.Drones
{
    public class MoveState<L, V> : MachineState<L, V>
    {
        private readonly IMovement movement;
        
        public MoveState(IMovement moveBehaviour)
        {
            movement = moveBehaviour;
        }

        public override void Start()
        {
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
    }
}