using Aspekt.Items;
using UnityEngine;

namespace Aspekt.Drones
{
    public class MainInventory : Inventory
    {
        public override ItemAddResult CanItemBeAdded(InventoryItem item)
        {
            return ItemAddResult.Success;
        }

        public override void OnItemAddedToSlot(InventoryItem item, Slot slot)
        {
            Debug.Log("item added to inventory: " + item.itemName);
        }

        public override void OnItemRemovedFromSlot(InventoryItem item, Slot slot)
        {
            Debug.Log("item removed from inventory: " + item.itemName);
        }

        public override void OnAddItemFailure(Slot slot, ItemAddResult result)
        {
            Debug.Log("failed to add item: " + result.ToString());
        }

        public override void OnSlotClicked(Slot slot)
        {
            // Do nothing in main inventory
        }
    }
}