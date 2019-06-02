namespace Aspekt.AI.Core
{
    public interface IStateMachine
    {
        /// <summary>
        /// Initialises the state machine
        /// </summary>
        void Init(IAIAgent agent);
        
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