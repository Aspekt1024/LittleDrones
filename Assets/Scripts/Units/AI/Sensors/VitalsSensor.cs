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
        [SerializeField] private int normalFuelPriority = 1;
        [SerializeField] private int lowFuelPriority = 100;
        [SerializeField] private int criticalFuelPriority = 1000;
#pragma warning restore 414
        
        private DroneVitals vitals;
        private MaintainFuelGoal fuelGoal;

        private bool isRefillingFromLow;
        
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
            CheckFuelLevels();
        }

        private void CheckFuelLevels()
        {
            if (fuelGoal == null)
            {
                fuelGoal = Agent.Goals.Get<MaintainFuelGoal>();
                if (fuelGoal == null) return;
            }
            
            float fuelPercent = vitals.CurrentFuel / vitals.FuelCapacity;

            if (fuelPercent < criticalFuelPercent)
            {
                fuelGoal.Priority = criticalFuelPriority;
            }
            else if (fuelPercent < lowFuelPercent)
            {
                isRefillingFromLow = true;
                fuelGoal.Priority = lowFuelPriority;
            }
            else if (fuelPercent < nominalFuelPercent)
            {
                fuelGoal.Priority = isRefillingFromLow ? lowFuelPriority : normalFuelPriority;
            }
            else
            {
                isRefillingFromLow = false;
            }
            
            Agent.Memory.Set(AIAttributes.MaintainFuelGoal, fuelPercent > nominalFuelPercent);
        }
    }
}