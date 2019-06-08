using System.Collections.Generic;

namespace Aspekt.AI
{
    /// <summary>
    /// Manages the AI agent's actions
    /// </summary>
    public interface IActionController<L, V>
    {
        /// <summary>
        /// Initialises the action controller
        /// </summary>
        /// <param name="agent">The parent AI agent</param>
        void Init(IAIAgent<L, V> agent);
        
        /// <summary>
        /// Returns a copy of the list of actions in the action controller
        /// </summary>
        List<IAIAction<L, V>> GetActions();
        
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
        void AddAction<TAction>() where TAction : IAIAction<L, V>, new();
        
        /// <summary>
        /// Removes an action of the given type, if it exists on the agent
        /// </summary>
        /// <typeparam name="TAction">The sensor type to remove</typeparam>
        void RemoveAction<TAction>() where TAction : IAIAction<L, V>;
    }
}