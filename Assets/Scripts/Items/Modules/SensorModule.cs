using System;
using System.Diagnostics.Contracts;
using Aspekt.AI;
using Aspekt.Items;
using UnityEngine;

namespace Aspekt.Drones
{
    public abstract class SensorModule : InventoryItem, IDroneModule
    {
        private Sensor<AIAttributes, object> sensor;
        
        public void AttachTo(DroneAIAgent agent)
        {
            agent.Sensors.AddSensor(sensor);
        }

        public void RemoveFrom(DroneAIAgent agent)
        {
            agent.Sensors.RemoveSensor(sensor);
        }

        private void Awake()
        {
            sensor = CreateSensor();
        }

        protected abstract Sensor<AIAttributes, object> CreateSensor();
    }
}