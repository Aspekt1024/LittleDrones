using System;

namespace Aspekt.AI
{
    public interface IStateMachine<L, V>
    {
        /// <summary>
        /// Called when the state machine has completed its queue
        /// </summary>
        event Action OnComplete;
        
        /// <summary>
        /// Adds the state to the queue
        /// </summary>
        void Enqueue(IMachineState<L, V> state);
        
        /// <summary>
        /// Starts the state machine
        /// </summary>
        void Start();
        
        /// <summary>
        /// Stops the state machine and all running states
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Pauses the state machine with the ability to resume
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Similar to MonoBehaviour.Update()
        /// </summary>
        /// <param name="deltaTime">Time since the last frame update</param>
        void Tick(float deltaTime);
    }
}