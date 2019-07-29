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
        private DroneParent droneParent;
        
        public void Init()
        {
            Debug.Log("Unit manager online");
            droneParent = Object.FindObjectOfType<DroneParent>();

            if (droneParent == null)
            {
                Debug.LogError($"{nameof(DroneParent)} was not found in the scene");
            }
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

        /// <summary>
        /// Initialises the unit with the default modules
        /// </summary>
        public void InitialiseDefaultUnit(IUnit unit)
        {
            switch (unit)
            {
                case null:
                    return;
                case Drone drone:
                    droneParent.InitialiseDroneModules(drone);
                    break;
            }
        }

        public void CreateUnit<T>(Vector3 spawnPosition) where T : IUnit
        {
            if (typeof(T) == typeof(Drone))
            {
                droneParent.CreateDrone(spawnPosition);
            }
        }
    }
}