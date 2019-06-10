using System;
using System.Collections.Generic;

namespace Aspekt.AI
{
    /// <summary>
    /// An action gives the agent the ability to carry out a specific task.
    /// Actions will have at least one Effect, and usually have one or more Precondition
    /// that need to be fulfilled before the action can be performed.
    /// </summary>
    public interface IAIAction<L, V>
    {
        /// <summary>
        /// Event called when the action was successful
        /// </summary>
        event Action OnActionSucceeded;

        /// <summary>
        /// Event called when the action failed
        /// </summary>
        event Action OnActionFailed;
        
        /// <summary>
        /// The cost of executing the action. A lower cost is more desirable
        /// </summary>
        float Cost { get; }
        
        /// <summary>
        /// Initialises the action
        /// </summary>
        /// <param name="agent">The parent AI agent</param>
        void Init(IAIAgent<L, V> agent);
        
        /// <summary>
        /// Tick is called once per frame, similar to MonoBehaviour.Update()
        /// </summary>
        /// <param name="deltaTime">The time since the last frame</param>
        void Tick(float deltaTime);

        /// <summary>
        /// Called by the AI agent's executor to begin execution of the action
        /// </summary>
        /// <returns>true if the action started successfully (preconditions were met, etc)</returns>
        bool Enter(IStateMachine<L, V> stateMachine);

        /// <summary>
        /// Returns the preconditions required for the action to run
        /// </summary>
        Dictionary<L, V> GetPreconditions();

        /// <summary>
        /// Checks preconditions that update over time.
        /// This is checked on action planning and for each tick of the AI agent (once per frame)
        /// </summary>
        bool CheckProceduralPreconditions();
        
        /// <summary>
        /// Returns the effects that result from the action being successfully completed
        /// </summary>
        Dictionary<L, V> GetEffects();
        
        /// <summary>
        /// Enables the action
        /// </summary>
        void Enable();
        
        /// <summary>
        /// Disables the action
        /// </summary>
        void Disable();

        /// <summary>
        /// Called when the action was removed from the AI agent
        /// </summary>
        void OnRemove();
    }
}