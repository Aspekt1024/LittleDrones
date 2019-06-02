using Aspekt.AI;

namespace Aspekt.Drones
{
    public interface IDroneModule
    {
        void AttachTo(AIAgent agent);
        void RemoveFrom(AIAgent agent);
    }
}