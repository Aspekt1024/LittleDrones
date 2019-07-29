using UnityEngine;

namespace Aspekt.Drones
{
    public enum BuildingTypes
    {
        None, ResourceDepot, CraftingPad
    }
    
    public abstract class BuildingBase : MonoBehaviour, IObject
    {
        public BuildingTypes buildingType;

        private void Start()
        {
            GameManager.Objects.AddBuilding(this);
        }

        public Transform Transform => transform;

        public override string ToString()
        {
            return buildingType.ToString();
        }
    }
}