using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Manages all objects in the game
    /// </summary>
    public class ObjectManager : IManager
    {
        public void Init()
        {
            Debug.Log("Object manager online");
        }
    }
}