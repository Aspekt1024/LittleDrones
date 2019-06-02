using UnityEngine;

namespace Aspekt.Items
{
    public interface ISlot
    {
        /// <summary>
        /// Initialises the slot
        /// </summary>
        void Init(Inventory inventory, Canvas topCanvas, int number);
        
        /// <summary>
        /// Adds the given item to the inventory
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>The result of trying to add the item</returns>
        ItemAddResult AddItem(InventoryItem item);
        
        /// <summary>
        /// Removes the item from the inventory
        /// </summary>
        void RemoveItem();

        /// <summary>
        /// Returns the item currently in the slot
        /// </summary>
        InventoryItem GetItem();

        /// <summary>
        /// Sets the slot graphic as selected
        /// </summary>
        void Select();

        /// <summary>
        /// Sets the slot graphic as unselected
        /// </summary>
        void Deselect();

        /// <summary>
        /// Sets the slot graphic as highlighted
        /// </summary>
        void Highlight();

        /// <summary>
        /// Removes highlighting from the slot graphic
        /// </summary>
        void Unhighlight();
    }
}