using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aspekt.Items
{
    public abstract class Inventory : MonoBehaviour
    {
        public int NumSlots = 20;
        public Slot SlotPrefab;
        public Canvas topCanvas;
        
        protected Slot[] slots;
        protected Slot selectedSlot;

        public abstract ItemAddResult CanItemBeAdded(InventoryItem item);
        public abstract void OnAddItemFailure(Slot slot, ItemAddResult result);

        private void Awake()
        {
            slots = GetComponentsInChildren<Slot>();
            for (int i = 0; i < NumSlots; i++)
            {
                slots[i].Init(this, topCanvas, i);
            }
        }

        public ItemAddResult AddItem(InventoryItem item)
        {
            var result = CanItemBeAdded(item);
            if (result != ItemAddResult.Success) return result;
            
            foreach (var slot in slots)
            {
                if (slot.IsPopulated) continue;
                return slot.AddItem(item);
            }
            return ItemAddResult.InventoryFull;
        }

        public void RemoveItem(InventoryItem item)
        {
            throw new NotImplementedException();
        }

        public virtual void OnSlotClicked(Slot slot, PointerEventData eventData)
        {
            if (selectedSlot != null)
            {
                selectedSlot.Deselect();
            }

            selectedSlot = slot;
            if (slot.IsPopulated)
            {
                slot.Select();
            }
        }
        
        public virtual void OnPointerEnter(Slot slot, PointerEventData eventData) { }
        public virtual void OnPointerExit(Slot slot, PointerEventData eventData) { }
        public virtual void OnItemAddedToSlot(InventoryItem item, Slot slot) { }
        public virtual void OnItemRemovedFromSlot(InventoryItem item, Slot slot) { }
    }
}