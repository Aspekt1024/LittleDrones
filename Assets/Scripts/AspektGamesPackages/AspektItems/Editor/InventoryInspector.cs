using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Aspekt.Items
{
    [CustomEditor(typeof(Inventory), true)]
    public class InventoryInspector : Editor
    {
        private Inventory inv;
        
        public override void OnInspectorGUI()
        {
            inv = (Inventory) target;

            int numSlots = EditorGUILayout.IntField("Num Slots", inv.NumSlots);
            if (numSlots < 1) numSlots = 1;
            if (numSlots > 100) numSlots = 100;
            inv.NumSlots = numSlots;
            
            SetSlotCount();

            var slotPrefab = (Slot)EditorGUILayout.ObjectField("Slot Prefab", inv.SlotPrefab, typeof(Slot), false);
            if (slotPrefab != inv.SlotPrefab)
            {
                inv.SlotPrefab = slotPrefab;
                ResetSlots();
            }
            
            base.OnInspectorGUI();
        }

        private void SetSlotCount()
        {
            if (inv.SlotPrefab == null) return;
            
            var slots = inv.GetComponentsInChildren<Slot>().ToList();
            int numSlots = slots.Count;
            
            if (numSlots < inv.NumSlots)
            {
                AddSlots(inv.NumSlots - numSlots);
                EditorUtility.SetDirty(inv);
            }
            else if (numSlots > inv.NumSlots)
            {
                RemoveSlots(slots, numSlots - inv.NumSlots);
                EditorUtility.SetDirty(inv);
            }
        }
        
        private void RemoveSlots(List<Slot> slots, int numToRemove)
        {
            int slotCount = slots.Count;
            for (int i = slotCount - 1; i >= slotCount - numToRemove; i--)
            {
                DestroyImmediate(slots[i].gameObject);
            }
        }

        private void AddSlots(int numToAdd)
        {
            if (inv.SlotPrefab == null) return;
            
            for (int i = 0; i < numToAdd; i++)
            {
                var newSlot = PrefabUtility.InstantiatePrefab(inv.SlotPrefab, inv.transform);
                newSlot.name = "Slot";
            }
        }

        private void ResetSlots()
        {
            var slots = inv.GetComponentsInChildren<Slot>();
            for (int i = slots.Length - 1; i >= 0; i--)
            {
                DestroyImmediate(slots[i].gameObject);
            }
            AddSlots(inv.NumSlots);
        }
    }
}