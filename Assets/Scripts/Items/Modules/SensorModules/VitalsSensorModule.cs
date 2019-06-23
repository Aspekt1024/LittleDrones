using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Vitals Sensor Module", menuName = "Drone/Sensor Module/Vitals Sensor")]
    public class VitalsSensorModule : SensorModule
    {
        public VitalsSensor sensor = new VitalsSensor();
        
        protected override Sensor<AIAttributes, object> CreateSensor()
        {
            return sensor;
        }
    }
}