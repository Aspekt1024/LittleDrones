using Aspekt.AI;

namespace Aspekt.Drones
{
    public interface IDroneModule
    {
        void AttachTo(DroneAIAgent agent);
        void RemoveFrom(DroneAIAgent agent);
    }
}