using Aspekt.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aspekt.Items
{
    public class Slot : MonoBehaviour, ISlot, IPointerClickHandler
    {
        [HideInInspector] public Canvas topCanvas;
        
        public int SlotNumber { get; private set; }
        
#pragma warning disable 649
        [SerializeField] private Image background;
        [SerializeField] private Image iconImage;
        
        [SerializeField] private Color emptyColor;
        [SerializeField] private Color populatedColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color highlightedColor;
#pragma warning restore 649

        private Inventory inventory;
        private InventoryItem item;

        private enum States
        {
            Selected, Unselected
        }
        private States state;

        public bool IsEmpty => item == null;
        public bool IsPopulated => item != null;

        public void Init(Inventory parent, Canvas canvasOnTop, int number)
        {
            inventory = parent;
            topCanvas = canvasOnTop;
            SlotNumber = number;
            
            SetEmpty();
            Deselect();
        }

        public ItemAddResult AddItem(InventoryItem itemToAdd)
        {
            if (IsPopulated) return ItemAddResult.AlreadyPopulated;
            var result = inventory.CanItemBeAdded(itemToAdd);
            if (result != ItemAddResult.Success) return result;

            item = itemToAdd;
            item.SetCurrentSlot(this);
            
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
            
            inventory.OnItemAddedToSlot(itemToAdd, this);
            Unhighlight();
            
            return ItemAddResult.Success;
        }

        public void RemoveItem()
        {
            var inventoryItem = item;
            SetEmpty();
            inventory.OnItemRemovedFromSlot(inventoryItem, this);
            state = States.Unselected;
            Unhighlight();
        }

        public InventoryItem GetItem() => item;

        public void Select()
        {
            state = States.Selected;
            background.color = selectedColor;
        }

        public void Deselect()
        {
            state = States.Unselected;
            background.color = item == null ? emptyColor : populatedColor;
        }

        public void Highlight()
        {
            background.color = highlightedColor;
        }

        public void Unhighlight()
        {
            if (state == States.Selected)
            {
                Select();
            }
            else
            {
                Deselect();
            }
        }

        private void SetEmpty()
        {
            item = default;
            background.color = emptyColor;
            iconImage.enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            inventory.OnSlotClicked(this);
        }
    }
}