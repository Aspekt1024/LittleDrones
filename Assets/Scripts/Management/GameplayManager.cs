using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Manages the gameplay
    /// </summary>
    public class GameplayManager : IManager
    {
        
        public void Init()
        {
            GameManager.Units.InitialiseDefaultUnit(Object.FindObjectOfType<Drone>());
        }

    }
}