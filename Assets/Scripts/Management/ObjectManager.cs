using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Manages all objects in the game
    /// </summary>
    public class ObjectManager : IManager
    {
        public readonly List<BuildingBase> Buildings = new List<BuildingBase>();
        
        public void Init()
        {
            
        }

        public void AddBuilding(BuildingBase building)
        {
            Buildings.Add(building);
        }

        public void RemoveBuilding(BuildingBase building)
        {
            Buildings.Remove(building);
        }

    }
}