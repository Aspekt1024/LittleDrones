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
            InitialiseDroneModules();
        }
        
        private void InitialiseDroneModules()
        {
            var drone = Object.FindObjectOfType<Drone>();
            if (drone == null) return;
            
            drone.AddSensor(Object.Instantiate(Resources.Load<SensorModule>("DroneModules/Sensors/ResourceSensor")));
            drone.AddSensor(Object.Instantiate(Resources.Load<SensorModule>("DroneModules/Sensors/BuildingSensor")));
            drone.AddSensor(Object.Instantiate(Resources.Load<SensorModule>("DroneModules/Sensors/ObjectSensor")));
            drone.AddSensor(Object.Instantiate(Resources.Load<SensorModule>("DroneModules/Sensors/VitalsSensor")));

            drone.AddAction(Object.Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/PickupItem")));
            drone.AddAction(Object.Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/FindResource")));
            drone.AddAction(Object.Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/StoreItem")));
            drone.AddAction(Object.Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/GatherFromDeposit")));
            drone.AddAction(Object.Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/FindDeposit")));
            drone.AddAction(Object.Instantiate(Resources.Load<ActionModule>("DroneModules/Actions/ConsumeFuel")));

            drone.AddGoal(Object.Instantiate(Resources.Load<GoalModule>("DroneModules/Goals/MaintainFuel")));
            drone.AddGoal(Object.Instantiate(Resources.Load<GoalModule>("DroneModules/Goals/GatherIronGoal")));
        }

    }
}