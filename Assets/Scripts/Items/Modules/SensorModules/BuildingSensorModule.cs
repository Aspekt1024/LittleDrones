using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [CreateAssetMenu(fileName = "New Building Sensor Module", menuName = "Drone/Sensor Module/Building Sensor")]
    public class BuildingSensorModule : SensorModule
    {
        public BuildingSensor buildingSensor = new BuildingSensor();
        
        protected override Sensor<AIAttributes, object> CreateSensor()
        {
            return buildingSensor;
        }
    }
}