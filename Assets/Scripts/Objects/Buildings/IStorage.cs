namespace Aspekt.Drones
{
    public interface IStorage
    {
        bool TakeItem(IGrabbableItem item);
    }
}