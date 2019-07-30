namespace Aspekt.Drones
{
    public enum AIAttributes
    {
        Invalid,
        
        ResourceGoalType,
        
        HasItemToGather,
        HasDepositToGather,
        
        ItemToGather,
        DepositToGather,
        
        IsHoldingFuel,
        IsHoldingResource,
        HeldItem,
        
        CraftingStationReady,
        TargetBuilding,
        
        // Goals
        HasGatheredResource,
        HasLowFuel,
        HasCraftedDrone,
        HasSuppliedCraftingStation,
    }
}