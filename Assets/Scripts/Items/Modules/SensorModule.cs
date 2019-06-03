using System;
using System.Diagnostics.Contracts;
using Aspekt.AI;
using Aspekt.Items;
using UnityEngine;

namespace Aspekt.Drones
{
    public enum SensorModules
    {
        ResourceScanner,
    }
    
    [CreateAssetMenu(fileName = "New Module", menuName = "Drone/Sensor Module")]
    public class SensorModule : InventoryItem, IDroneModule
    {
        public SensorModules sensorType;

        public void AttachTo(DroneAIAgent agent)
        {
            switch (sensorType)
            {
                case SensorModules.ResourceScanner:
                    agent.Sensors.AddSensor<ResourceSensor>();
                    break;
                default:
                    Debug.LogError("invalid sensor type: " + sensorType);
                    break;
            }
        }

        public void RemoveFrom(DroneAIAgent agent)
        {
            switch (sensorType)
            {
                case SensorModules.ResourceScanner:
                    agent.Sensors.RemoveSensor<ResourceSensor>();
                    break;
                default:
                    break;
            }
        }
        
    }
}