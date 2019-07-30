using System;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class BuildingSensor : Sensor<AIAttributes, object>
    {
        protected override void OnTick(float deltaTime) { }
        
        public BuildingBase FindClosestBuilding(BuildingTypes type, Vector3 pos)
        {
            var distance = float.MaxValue;
            BuildingBase building = null;

            foreach (var b in GameManager.Objects.Buildings)
            {
                if (b.buildingType != type) continue;
                
                var dist = Vector3.Distance(pos, b.Transform.position);
                if (!(dist < distance)) continue;
                
                distance = dist;
                building = b;
            }

            return building;
        }

        public BuildingBase FindClosestBuilding<T>(BuildingTypes type, Vector3 pos, Predicate<T> predicate) where T : BuildingBase
        {
            var distance = float.MaxValue;
            BuildingBase building = null;

            foreach (var b in GameManager.Objects.Buildings)
            {
                if (b.buildingType != type) continue;
                if (!predicate.Invoke((T)b)) continue;
                
                var dist = Vector3.Distance(pos, b.Transform.position);
                if (!(dist < distance)) continue;
                
                distance = dist;
                building = b;
            }

            return building;
        }
    }
}