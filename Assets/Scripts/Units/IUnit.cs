namespace Aspekt.Drones
{
    public interface IUnit
    {
        /// <summary>
        /// The ability manager for the unit
        /// </summary>
        IAbilityManager Abilities { get; }
        
        /// <summary>
        /// Removes the unit from the game
        /// </summary>
        void Remove();

        /// <summary>
        /// Returns the name of the unit
        /// </summary>
        /// <returns>the name of the unit</returns>
        string GetName();
    }
}