namespace Aspekt.AI.Core
{
    public interface IMemory
    {
        /// <summary>
        /// Initialises the Memory component of the agent
        /// </summary>
        /// <param name="agent">The agent the memory belongs to</param>
        void Init(IAIAgent agent);
        
        /// <summary>
        /// Wipes the memory of the agent
        /// </summary>
        void Reset();
    }
}