using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Manages all units in the game
    /// </summary>
    public class UnitManager : IManager
    {
        private readonly List<IUnit> units = new List<IUnit>();
        
        public void Init()
        {
            Debug.Log("Unit manager online");
        }

        public void RegisterUnit(IUnit unit)
        {
            units.Add(unit);
        }

        public void UnregisterUnit(IUnit unit)
        {
            if (units.Contains(unit))
            {
                units.Remove(unit);
            }
        }
    }
}