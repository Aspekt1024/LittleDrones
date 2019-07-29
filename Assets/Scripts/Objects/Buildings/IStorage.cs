namespace Aspekt.Drones
{
    public interface IStorage
    {
        bool TakeItem(IGrabbableItem item);

        /// <summary>
        /// Returns the number of the given type of resources
        /// </summary>
        int GetResourceCount(ResourceTypes type);
    }
}