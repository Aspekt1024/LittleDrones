using UnityEngine;

namespace Aspekt.Drones
{
    public interface ICanAnimate
    {
        /// <summary>
        /// Retrieves the animator
        /// </summary>
        /// <returns></returns>
        Animator GetAnimator();
    }
}