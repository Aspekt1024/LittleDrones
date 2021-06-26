using UnityEngine;

namespace Aspekt.Items
{
    public abstract class InventoryItem : ScriptableObject
    {
        public string itemName;
        public string description;
        public Sprite icon;

        private Slot currentSlot;

        public Slot GetCurrentSlot() => currentSlot;

        public void SetCurrentSlot(Slot slot)
        {
            currentSlot = slot;
        }
    }
}