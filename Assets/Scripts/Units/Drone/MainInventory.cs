using Aspekt.Items;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aspekt.Drones
{
    public class MainInventory : Inventory
    {
        public override ItemAddResult CanItemBeAdded(InventoryItem item)
        {
            return ItemAddResult.Success;
        }

        public override void OnAddItemFailure(Slot slot, ItemAddResult result)
        {
            Debug.Log("failed to add item: " + result.ToString());
        }

        public override void OnSlotClicked(Slot slot, PointerEventData eventData)
        {
            // TODO implement picked up?
        }

        public override void OnPointerEnter(Slot slot, PointerEventData eventData)
        {
            base.OnPointerEnter(slot, eventData);
            
            if (slot.IsEmpty) return;
            
            var tooltip = GameManager.UI.Get<TooltipUI>();

            var details = new TooltipUI.Details(slot.GetItem());
            tooltip.Populate(details);
            tooltip.PositionNear(slot.transform);
            tooltip.Open(0.1f);
        }

        public override void OnPointerExit(Slot slot, PointerEventData eventData)
        {
            GameManager.UI.Get<TooltipUI>().Close(0.1f);
        }
    }
}