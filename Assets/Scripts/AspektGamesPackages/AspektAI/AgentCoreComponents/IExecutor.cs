using System;
using System.Collections.Generic;

namespace Aspekt.AI
{
    public interface IExecutor<L, V>
    {
        /// <summary>
        /// Called when an action plan has finished
        /// </summary>
        event Action OnActionPlanComplete;
        
        /// <summary>
        /// Called once per frame
        /// </summary>
        void Tick(float deltaTime);

        /// <summary>
        /// Executes the new action plan that fulfils the given goal
        /// </summary>
        void ExecutePlan(Queue<IAIAction<L, V>> newActionPlan, IAIGoal<L, V> goal);

        /// <summary>
        /// Stops the execution of the current action plan
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses the execution of the running action plan
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the execution of the running action plan
        /// </summary>
        void Resume();

        /// <summary>
        /// Returns the status of the executor
        /// </summary>
        string GetStatus();
    }
}