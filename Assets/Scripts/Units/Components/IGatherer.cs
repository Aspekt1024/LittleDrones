using System;

namespace Aspekt.Drones
{
    public interface IGatherer
    {
        /// <summary>
        /// Instructs the gatherer to begin gathering the target gatherable
        /// </summary>
        /// <param name="target">The target gatherable</param>
        /// <param name="completionCallback">Called once the gatherer has completed gathering from the gatherable</param>
        /// <returns>true if the gatherable can be gathered at this time</returns>
        bool StartGathering(IGatherable target, Action<IGatherable> completionCallback);
        
        /// <summary>
        /// Instructs the gatherer to stop gathering the target gatherable
        /// </summary>
        void StopGathering();
        
        /// <summary>
        /// Called once per frame
        /// </summary>
        /// <param name="deltaTime">The time since the last frame update</param>
        void Tick(float deltaTime);
    }
}