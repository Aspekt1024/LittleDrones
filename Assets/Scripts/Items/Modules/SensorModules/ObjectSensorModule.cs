using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Object Sensor Module", menuName = "Drone/Sensor Module/Object Sensor")]
    public class ObjectSensorModule : SensorModule
    {
        public ObjectSensor objectSensor = new ObjectSensor();
        
        protected override Sensor<AIAttributes, object> CreateSensor()
        {
            return objectSensor;
        }
    }
}