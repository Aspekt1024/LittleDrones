using UnityEngine;

namespace Aspekt.Drones
{
    public interface IMoveable
    {
        IMovement GetMovement();
    }
}