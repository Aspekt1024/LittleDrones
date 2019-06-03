namespace Aspekt.AI
{
    public interface IAIAgent<T, R>
    {
        /// <summary>
        /// Initialises the agent
        /// </summary>
        void Init();
        
        /// <summary>
        /// Forces recalculation of the action path for the allocated goals
        /// </summary>
        void RefreshActions();

        /// <summary>
        /// Tells the agent to start operation, or resume if in paused state
        /// </summary>
        void Run();

        /// <summary>
        /// Pauses operation of the agent.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops the agent's AI completely.
        /// </summary>
        void Stop();
    }
}