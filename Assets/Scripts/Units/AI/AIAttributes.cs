namespace Aspekt.Drones
{
    public enum AIAttributes
    {
        Invalid,
        
        ResourceGoalType,
        HasItemToGather,
        ItemToGather,
        
        IsHoldingItem,
        HeldItem,
        
        // Abilities
        CanMove,
        CanPickupItems,
        
        // Sensors
        HasResourceSensor,
        HasBuildingSensor,
        HasGatheredResource
    }
}