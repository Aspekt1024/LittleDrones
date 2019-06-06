namespace Aspekt.AI
{
    public interface IStateMachine<L, V>
    {
        /// <summary>
        /// Adds the state to the queue
        /// </summary>
        void Enqueue(IMachineState<L, V> state);
        
        /// <summary>
        /// Creates a new instance of a state of the given type and adds it to the queue
        /// </summary>
        /// <typeparam name="T">The type of the machine state to add to the queue</typeparam>
        /// <returns>The new machine state instance</returns>
        T AddState<T>() where T : IMachineState<L, V>, new();
        
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