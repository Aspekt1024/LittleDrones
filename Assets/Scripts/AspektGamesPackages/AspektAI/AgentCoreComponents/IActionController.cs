using System.Collections.Generic;

namespace Aspekt.AI.Core
{
    /// <summary>
    /// Manages the AI agent's actions
    /// </summary>
    public interface IActionController<T, R>
    {
        /// <summary>
        /// Initialises the action controller
        /// </summary>
        /// <param name="agent">The parent AI agent</param>
        /// <param name="memory">the AI's memory module</param>
        void Init(IAIAgent<T, R> agent, IMemory<T, R> memory);
        
        /// <summary>
        /// Returns a copy of the list of actions in the action controller
        /// </summary>
        List<IAIAction<T, R>> GetActions();
        
        /// <summary>
        /// Disables all the actions on the agent
        /// </summary>
        void DisableActions();
        
        /// <summary>
        /// Enables all the actions on the agent
        /// </summary>
        void EnableActions();
        
        /// <summary>
        /// Adds an action of the given type to the agent
        /// </summary>
        /// <typeparam name="TAction">The sensor type to add</typeparam>
        void AddAction<TAction>() where TAction : IAIAction<T, R>, new();
        
        /// <summary>
        /// Removes an action of the given type, if it exists on the agent
        /// </summary>
        /// <typeparam name="TAction">The sensor type to remove</typeparam>
        void RemoveAction<TAction>() where TAction : IAIAction<T, R>;
    }
}