using Aspekt.AI;

namespace Aspekt.Drones
{
    public class BuildingSensor : Sensor<AIAttributes, object>
    {
        public override AIAttributes[] Effects { get; } = { AIAttributes.HasBuildingSensor};

        protected override void OnTick(float deltaTime) { }
        protected override void OnRemove() { }

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