using System.Collections.Generic;
using Aspekt.Drones;
using Aspekt.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aspekt.UI
{
    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private GraphicRaycaster raycaster;
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        private Slot parentSlot;
        private Slot hoveredObject;

        private Transform parentTransform;
        private Canvas topCanvas;

        private void Start()
        {
            parentSlot = GetComponentInParent<Slot>();
            topCanvas = parentSlot.topCanvas;
            raycaster = GetComponentInParent<GraphicRaycaster>();
            parentTransform = transform.parent;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            parentSlot.OnPointerClick(eventData);
            hoveredObject = parentSlot;
            if (topCanvas != null)
            {
                transform.SetParent(topCanvas.transform);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
            raycaster.Raycast(eventData, raycastResults);

            foreach (var result in raycastResults)
            {
                if (result.gameObject.CompareTag("InventorySlot"))
                {
                    if (hoveredObject != null)
                    {
                        hoveredObject.Unhighlight();
                    }
                    hoveredObject = result.gameObject.GetComponent<Slot>();
                    hoveredObject.Highlight();
                    raycastResults.Clear();
                    return;
                }
            }
            
            ClearHighlights();
            raycastResults.Clear();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            raycaster.Raycast(eventData, raycastResults);
            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            ClearHighlights();

            foreach (var result in raycastResults)
            {
                // TODO delegate to inventory?
                if (result.gameObject.CompareTag("InventorySlot"))
                {
                    var newSlot = result.gameObject.GetComponent<Slot>();
                    if (parentSlot == newSlot) return;
                    
                    var addResult = newSlot.AddItem(parentSlot.GetItem());
                    if (addResult == ItemAddResult.Success)
                    {
                        parentSlot.RemoveItem();
                        newSlot.OnPointerClick(eventData);
                    }
                    else
                    {
                        newSlot.GetComponentInParent<Inventory>().OnAddItemFailure(parentSlot, addResult);
                    }
                    
                    break;
                }
                
                // TODO add ability to drop on ground?
            }
            raycastResults.Clear();
        }
        
        private void ClearHighlights()
        {
            if (hoveredObject == null) return;
            hoveredObject.Unhighlight();
            hoveredObject = null;
        }
    }
}