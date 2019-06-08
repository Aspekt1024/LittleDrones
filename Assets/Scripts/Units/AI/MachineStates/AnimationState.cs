using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class AnimationState<L, V> : MachineState<L, V>
    {
        private readonly Animator animator;
        private readonly string animation;
        
        public AnimationState(Animator animator, string animation)
        {
            this.animator = animator;
            this.animation = animation;
        }

        public override void Start()
        {
            animator.Play(animation, 0, 0f);
        }

        public override void Pause()
        {
            animator.StopPlayback();
        }

        public override void Stop()
        {
            animator.StopPlayback();
        }
    }
}