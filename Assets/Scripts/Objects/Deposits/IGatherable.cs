namespace Aspekt.Drones
{
    public interface IGatherable
    {
        /// <summary>
        /// Registers an active gatherer to the gatherable object
        /// </summary>
        /// <param name="gatherer">The gatherer to add</param>
        /// <returns>true if the gatherer was successfully added</returns>
        bool AddGatherer(IGatherer gatherer);
        
        /// <summary>
        /// Unregisters a gatherer from the gatherable object
        /// </summary>
        /// <param name="gatherer">The gatherer to add</param>
        void RemoveGatherer(IGatherer gatherer);

        /// <summary>
        /// Increments the gathering percentage based on the gatherer's skill
        /// </summary>
        /// <param name="currentPercent">The current gathering percentage</param>
        /// <param name="gatherSkill">The skill of the gatherer for this gatherable</param>
        /// <param name="deltaTime">The time passed since the last frame</param>
        /// <returns>The updated gathering percentage</returns>
        float IncrementGatherPercent(float currentPercent, float gatherSkill, float deltaTime);
        
        /// <summary>
        /// Gets an item of the type specified by the gatherable object
        /// </summary>
        IGrabbableItem GetItem();
        
        /// <summary>
        /// Same as <see cref="GetItem"/> but ignores item availability
        /// </summary>
        IGrabbableItem GetItemIgnoreAvailability();

    }
}