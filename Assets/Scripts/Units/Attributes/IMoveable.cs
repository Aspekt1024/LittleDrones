using UnityEngine;
using Aspekt.AI;

namespace Aspekt.Drones
{
    public interface IMoveable
    {
        IMovement GetMovement();
    }
}