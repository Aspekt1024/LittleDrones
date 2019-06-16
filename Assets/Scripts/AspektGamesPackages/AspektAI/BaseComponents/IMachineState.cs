using System;

namespace Aspekt.AI
{
    public interface IMachineState<L, V>
    {
        event Action OnComplete;
        
        /// <summary>
        /// Called once per frame
        /// </summary>
        void Tick(float deltaTime);
        
        /// <summary>
        /// Begins/Resumes the state
        /// </summary>
        void Start();
        
        /// <summary>
        /// Pauses the state
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Stops the state
        /// </summary>
        void Stop();
    }
}