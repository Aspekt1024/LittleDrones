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
        IsHoldingItem,
        HeldItem,
        
        // Goals
        HasGatheredResource,
        HasLowFuel,
    }
}