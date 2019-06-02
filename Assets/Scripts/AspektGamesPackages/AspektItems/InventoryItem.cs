using UnityEngine;

namespace Aspekt.Items
{
    public abstract class InventoryItem : ScriptableObject
    {
        public string description;
        public string itemName;
        public Sprite icon;

        private Slot currentSlot;

        public Slot GetCurrentSlot() => currentSlot;

        public void SetCurrentSlot(Slot slot)
        {
            currentSlot = slot;
        }
    }
}