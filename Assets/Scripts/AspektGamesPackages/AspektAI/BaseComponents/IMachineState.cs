using System;

namespace Aspekt.AI
{
    public interface IMachineState<L, V>
    {
        event Action OnComplete;
        
        /// <summary>
        /// Initialises the state (used on startup)
        /// </summary>
        void Init(IAIAgent<L, V> parentAgent);
        
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