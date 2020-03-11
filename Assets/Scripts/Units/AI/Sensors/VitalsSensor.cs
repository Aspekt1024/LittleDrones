using System;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [Serializable]
    public class VitalsSensor : Sensor<AIAttributes, object>
    {
#pragma warning disable 414
        [SerializeField] private float lowFuelPercent = 0.5f;
        [SerializeField] private float criticalFuelPercent = 0.1f;
        [SerializeField] private float nominalFuelPercent = 0.8f; 
#pragma warning restore 414
        
        private DroneVitals vitals;

        private bool isFuelLow;
        
        protected override void OnInit()
        {
            vitals = Agent.Owner.GetComponent<Drone>()?.Vitals;
            if (vitals == null)
            {
                Debug.LogError("No vitals object found on " + Agent.Owner.name);
            }
        }
        
        protected override void OnTick(float deltaTime)
        {
            float fuelPercent = vitals.CurrentFuel / vitals.FuelCapacity;

            if (isFuelLow && fuelPercent < 0.8f)
            {
                Agent.Memory.Set(AIAttributes.HasLowFuel, true);
            }
            if (fuelPercent < 0.5f)
            {
                Agent.Memory.Set(AIAttributes.HasLowFuel, true);
                isFuelLow = true;
            }
            if (fuelPercent < 0.1f)
            {
                // TODO increase maintain fuel goal priority
            }

            if (fuelPercent > 0.8f)
            {
                Agent.Memory.Set(AIAttributes.HasLowFuel, false);
                isFuelLow = false;
            }
            
            Debug.Log(Agent.Memory.Get(AIAttributes.HasLowFuel));
        }
    }
}