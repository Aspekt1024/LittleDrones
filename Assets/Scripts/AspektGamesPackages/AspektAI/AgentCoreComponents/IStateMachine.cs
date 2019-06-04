using System.Collections.Generic;

namespace Aspekt.AI.Core
{
    public interface IStateMachine<T, R>
    {
        /// <summary>
        /// Initialises the state machine
        /// </summary>
        void Init(IAIAgent<T, R> agent);
        
        /// <summary>
        /// Starts the state machine
        /// </summary>
        bool Start();
        
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

        /// <summary>
        /// Sets the action queue
        /// </summary>
        void SetQueue(Queue<IAIAction<T, R>> queue);
    }
}