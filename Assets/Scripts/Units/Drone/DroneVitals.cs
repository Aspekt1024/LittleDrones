using System;
using UnityEngine;

namespace Aspekt.Drones
{
    [Serializable]
    public class DroneVitals
    {
#pragma warning disable 649
        [SerializeField] private float fuelCapacity = 10f;
        [SerializeField] private float initialFuel = 3f;
        [SerializeField] private float fuelUsageRate = 0.1f;
#pragma warning restore 649
        
        public float CurrentFuel { get; private set; }
        public float FuelCapacity => fuelCapacity;

        public bool IsConsumingFuel { get; set; }

        public DroneVitals()
        {
            CurrentFuel = initialFuel;
        }

        public void AddFuel(float amount)
        {
            CurrentFuel = Mathf.Min(fuelCapacity, CurrentFuel + amount);
        }

        public void SetUsageRate(float newRate)
        {
            fuelUsageRate = newRate;
        }

        public void Tick(float deltaTime)
        {
            if (!IsConsumingFuel) return;
            CurrentFuel -= deltaTime * fuelUsageRate;
        }

    }
}