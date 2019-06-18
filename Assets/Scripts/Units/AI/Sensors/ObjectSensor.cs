using UnityEngine;

namespace Aspekt.Drones
{
    public class ObjectSensor : LayerSensorBase
    {
        protected override LayerMask ObjectLayerMask { get; set; }

        protected override void OnInit()
        {
            ObjectLayerMask = 1 << LayerMask.NameToLayer("Resource");
        }
    }
}