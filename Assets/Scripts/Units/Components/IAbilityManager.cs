namespace Aspekt.Drones
{
    public interface IAbilityManager
    {
        /// <summary>
        /// Adds an ability to the ability list. This assumes the ability has been initialised and setup
        /// </summary>
        /// <param name="ability">The initialised ability</param>
        /// <returns>true if the ability was successfully added</returns>
        bool AddAbility(IAbility ability);
        
        /// <summary>
        /// Returns the ability of the given type, if it exists in the ability list
        /// </summary>
        T GetAbility<T>() where T : IAbility;

        /// <summary>
        /// Checks if the ability of the given type exists in the ability list and is enabled
        /// </summary>
        bool HasAbility<T>() where T : IAbility;

    }
}