using System;
using Aspekt.AI.Internal;
using UnityEngine;

namespace Aspekt.AI
{
    public interface IAIAgent<L, V>
    {
        /// <summary>
        /// The memory component of the AI agent
        /// </summary>
        IMemory<L, V> Memory { get; }
        
        /// <summary>
        /// The Transform of the AI agent
        /// </summary>
        Transform Transform { get; }
        
        /// <summary>
        /// The Owner of the AI agent
        /// </summary>
        GameObject Owner { get; }
        
        /// <summary>
        /// The action controller of the AI agent
        /// </summary>
        IActionController<L, V> Actions { get; }
        
        /// <summary>
        /// The sensor controller of the AI agent
        /// </summary>
        ISensorController<L, V> Sensors { get; }
        
        /// <summary>
        /// The goals controller of the AI agent
        /// </summary>
        IGoalController<L, V> Goals { get; }
        
        AILogger Logger { get; }
        
        /// <summary>
        /// Initialises the agent
        /// </summary>
        void Init(GameObject owner);

        /// <summary>
        /// Tells the agent to start operation
        /// </summary>
        void Run();

        /// <summary>
        /// Flag a new goal calculation to take place
        /// </summary>
        void QueueGoalCalculation();

        /// <summary>
        /// Calculates a goal calculation without automatically starting it
        /// </summary>
        void CalculateGoalDryRun(Action<ActionPlan<L, V>> callback);

        /// <summary>
        /// Run through an action plan that was previously calculated
        /// </summary>
        void RunActionPlan(ActionPlan<L, V> actionPlan);

        /// <summary>
        /// Pauses operation of the AI agent
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the operation of the AI agent
        /// </summary>
        void Resume();

        /// <summary>
        /// Stops the agent's AI completely
        /// </summary>
        void Stop();

        /// <summary>
        /// Logs an informational message relating to the AI agent's behaviour
        /// </summary>
        /// <param name="parent">the parent class (usually 'this')</param>
        /// <param name="message">The message to log</param>
        void LogInfo<T>(T parent, string message);

        /// <summary>
        /// Logs key information relating to the AI agent's behaviour
        /// </summary>
        /// <param name="parent">the parent class (usually 'this')</param>
        /// <param name="message">The message to log</param>
        void LogKeyInfo<T>(T parent, string message);

        /// <summary>
        /// Logs debug trace messages relating to the AI agent's behaviour
        /// </summary>
        /// <param name="parent">the parent class (usually 'this')</param>
        /// <param name="message">The message to log</param>
        void LogTrace<T>(T parent, string message);
    }
}