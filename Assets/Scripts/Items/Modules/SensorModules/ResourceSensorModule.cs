using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Resource Sensor Module", menuName = "Drone/Sensor Module/Resource Sensor")]
    public class ResourceSensorModule : SensorModule
    {
        public ResourceSensor resourceSensor = new ResourceSensor();
        
        protected override Sensor<AIAttributes, object> CreateSensor()
        {
            return resourceSensor;
        }
    }
}