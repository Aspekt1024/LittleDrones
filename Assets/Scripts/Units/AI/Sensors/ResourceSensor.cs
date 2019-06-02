using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{

    public class ResourceSensor : Sensor
    {
        private const float DetectionRadius = 5f;
        private const float RefreshRate = 1f;
        private const bool RequireLineOfSight = false;
        
        private float timeLastSensed;

        
        protected override void OnInit()
        {
            timeLastSensed = Time.time;
        }

        protected override void Sense(float deltaTime)
        {
            if (Time.time > timeLastSensed + RefreshRate)
            {
                timeLastSensed = Time.time;
                FindResources();
            }
        }

        protected override void OnRemove()
        {
            // TODO remove objects from memory
        }

        private void FindResources()
        {
            //Debug.Log("sensing resources");
            
            // TODO spherecast 
            // TODO if found, update memory
        }
    }
}