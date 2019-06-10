using Aspekt.AI;

namespace Aspekt.Drones
{
    public class BuildingSensor : Sensor<AIAttributes, object>
    {
        protected override void OnTick(float deltaTime) { }
        
        public BuildingBase FindClosestBuilding(BuildingTypes type)
        {
            foreach (var building in GameManager.Objects.Buildings)
            {
                if (building.buildingType == type) return building;
            }

            return null;
        }
    }
}