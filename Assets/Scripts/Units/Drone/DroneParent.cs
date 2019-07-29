using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Manages the creation of drones
    /// </summary>
    public class DroneParent : MonoBehaviour
    {
        [SerializeField] private GameObject dronePrefab;

        /// <summary>
        /// Creates a drone at the specified position
        /// </summary>
        /// <param name="spawnPosition"></param>
        public void CreateDrone(Vector3 spawnPosition)
        {
            var droneObject = Instantiate(dronePrefab, spawnPosition, Quaternion.identity, transform);
            var drone = droneObject.GetComponent<Drone>();
            InitialiseDroneModules(drone);
        }

        /// <summary>
        /// Initialises the drone modules with a preset list of modules
        /// </summary>
        public void InitialiseDroneModules(Drone drone)
        {
            drone.AddSensor(Instantiate(Resources.Load<SensorModule>("DroneModules/Sensors/ResourceSensor")));
            drone.AddSensor(Instantiate(Resources.Load<SensorModule>("DroneModules/Sensors/BuildingSensor")));
            drone.AddSensor(Instantiate(Resources.Load<SensorModule>("DroneModules/Sensors/ObjectSensor")));
            drone.AddSensor(Instantiate(Resources.Load<SensorModule>("DroneModules/Sensors/VitalsSensor")));

            drone.AddAction(Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/PickupItem")));
            drone.AddAction(Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/FindResource")));
            drone.AddAction(Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/StoreItem")));
            drone.AddAction(Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/GatherFromDeposit")));
            drone.AddAction(Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/FindDeposit")));
            drone.AddAction(Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/ConsumeFuel")));

            drone.AddGoal(Instantiate(Resources.Load<GoalModule>("DroneModules/Goals/MaintainFuel")));
            drone.AddGoal(Instantiate(Resources.Load<GoalModule>("DroneModules/Goals/GatherIronGoal")));
        }
    }
}