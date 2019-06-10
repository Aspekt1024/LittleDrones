using System.Collections.Generic;

namespace Aspekt.AI.Internal
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
        /// Adds the given action to the AI agent. Replaces actions of the same type that exist on the AI agent already
        /// </summary>
        void AddAction(IAIAction<L, V> action);
        
        /// <summary>
        /// Removes the action from the AI agent
        /// </summary>
        /// <param name="action"></param>
        void RemoveAction(IAIAction<L, V> action);
    }
}