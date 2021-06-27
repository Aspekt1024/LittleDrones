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
        
        IsHoldingItem,
        HeldItem,
        
        CraftingStationReady,
        TargetBuilding,
        
        // Goals
        GatherResourceGoal,
        MaintainFuelGoal,
        CraftDroneGoal,
        SupplyCraftingStationGoal,
    }
}