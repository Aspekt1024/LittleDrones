using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{

    public class ResourceSensor : Sensor<AIAttributes, object>
    {
        private const float DetectionRadius = 5f;
        private const float RefreshRate = 1f;
        private const bool RequireLineOfSight = false;
        
        private float timeLastSensed;
        
        protected override void OnInit()
        {
            timeLastSensed = Time.time;
        }

        protected override void OnTick(float deltaTime)
        {
            if (Time.time > timeLastSensed + RefreshRate)
            {
                timeLastSensed = Time.time;
                Sense();
            }
        }

        public override void Sense()
        {
            FindResources();
        }

        protected override void OnRemove()
        {
            // TODO remove objects from memory
        }

        protected override void OnEnable()
        {
            // Once enabled, run the cooldown time before searching again
            timeLastSensed = Time.time;
        }

        private void FindResources()
        {
            //Debug.Log("sensing resources");
            
            // TODO spherecast 
            // TODO if found, update memory
        }
    }
}