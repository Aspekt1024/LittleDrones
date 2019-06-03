using System.Collections.Generic;
using Aspekt.AI.Core;

namespace Aspekt.AI
{
    /// <summary>
    /// An action gives the agent the ability to carry out a specific task.
    /// Actions will have at least one Outcome, and usually have one or more Prerequisites
    /// that need to be fulfilled before the action can be performed.
    /// </summary>
    public interface IAIAction<T, R>
    {
        /// <summary>
        /// Initialises the action
        /// </summary>
        /// <param name="agent">The parent AI agent</param>
        /// <param name="memory">The memory module</param>
        void Init(IAIAgent<T, R> agent, IMemory<T, R> memory);
        
        /// <summary>
        /// Tick is called once per frame, similar to MonoBehaviour.Update()
        /// </summary>
        /// <param name="deltaTime">The time since the last frame</param>
        void Tick(float deltaTime);

        /// <summary>
        /// Starts the action
        /// </summary>
        void Begin();

        Dictionary<T, R> GetPrerequisites();
        
        Dictionary<T, R> GetOutcomes();

        /// <summary>
        /// Enables the action
        /// </summary>
        void Enable();
        
        /// <summary>
        /// Disables the action
        /// </summary>
        void Disable();
        
        /// <summary>
        /// Used to clean up the action upon removal
        /// </summary>
        void Remove();
    }
}