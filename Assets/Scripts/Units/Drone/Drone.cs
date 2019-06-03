using UnityEngine;
using Aspekt.AI;
using Aspekt.Items;

namespace Aspekt.Drones
{
    public class Drone : UnitBase
    {
#pragma warning disable 649
        [SerializeField] private DroneAIAgent ai;
        [SerializeField] private MainInventory inventory;
        [SerializeField] private SensorInventory sensorSlots;
        [SerializeField] private ActionInventory actionSlots;
#pragma warning restore 649

        private void Awake()
        {
            ai.Init();
            sensorSlots.Init(ai);
            actionSlots.Init(ai);
        }

        private void Start()
        {
            ai.Run();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var unitUI = GameManager.UI.Get<UnitUI>();
                if (unitUI.IsOpen)
                {
                    unitUI.Close();
                }
                else
                {
                    var details = new UnitUI.Details()
                    {
                        Unit = this
                    };
                    unitUI.Populate(details);
                    unitUI.Open();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var modulePrefab = Resources.Load<SensorModule>("DroneModules/Sensors/ResourceScanner");
                var module = Instantiate(modulePrefab);
                inventory.AddItem(module);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                var modulePrefab = Resources.Load<ActionModule>("DroneModules/Actions/GatherResource");
                var module = Instantiate(modulePrefab);
                inventory.AddItem(module);
            }
        }

        private void OnMainInventoryItemAdded(InventoryItem item)
        {
            Debug.Log("item added: " + item.itemName);
        }

        private void OnMainInventoryItemRemoved(InventoryItem item)
        {
            Debug.Log(("item removed: " + item.itemName));
        }
    }
}

