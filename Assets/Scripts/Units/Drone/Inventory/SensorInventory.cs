using Aspekt.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aspekt.Drones
{
    public class SensorInventory : Inventory
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI moduleText;
#pragma warning restore 649
        
        private DroneAIAgent ai;
        
        public void Init(DroneAIAgent agent)
        {
            ai = agent;
        }
        
        public override ItemAddResult CanItemBeAdded(InventoryItem item)
        {
            if (!(item is SensorModule module)) return ItemAddResult.InvalidType;
            
            foreach (var slot in slots)
            {
                if (slot.IsEmpty || slot == item.GetCurrentSlot()) continue;
                var existingModule = (SensorModule)slot.GetItem();
                if (existingModule.GetType() == module.GetType()) return ItemAddResult.DuplicateType;
            }

            return ItemAddResult.Success;
        }

        public override void OnItemAddedToSlot(InventoryItem item, Slot slot)
        {
            if (!(item is SensorModule module)) return;
            module.AttachTo(ai);
        }

        public override void OnItemRemovedFromSlot(InventoryItem item, Slot slot)
        {
            if (item is SensorModule module)
            {
                module.RemoveFrom(ai);
            }
        }
        
        public override void OnSlotClicked(Slot slot, PointerEventData eventData)
        {
            base.OnSlotClicked(slot, eventData);

            if (slot.IsEmpty)
            {
                slot.Select();
                // TODO show details for selecting a slot
                moduleText.text = "put a sensor module here";
            }
            else
            {
                moduleText.text = slot.GetItem().itemName + "\n\n" + slot.GetItem().description;
            }
        }

        public override void OnAddItemFailure(Slot slot, ItemAddResult result)
        {
            moduleText.text = "Error: " + result;
            
            if (selectedSlot != null)
            {
                selectedSlot.Deselect();
                selectedSlot = null;
            }
        }
    }
}