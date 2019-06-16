using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class AnimationState<L, V> : MachineState<L, V>
    {
        private readonly Animator animator;

        public AnimationState(IAIAgent<L, V> agent, Animator animator) : base(agent)
        {
            this.animator = animator;
        }
        
        public override void Start()
        {
        }

        public override void Pause()
        {
            animator.StopPlayback();
        }

        public override void Stop()
        {
            animator.StopPlayback();
        }

        public void PlayAnimation(string animation)
        {
            animator.Play(animation, 0, 0f);
        }
    }
}