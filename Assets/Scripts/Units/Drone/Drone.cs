﻿using UnityEngine;
using Aspekt.AI;
using Aspekt.Items;

namespace Aspekt.Drones
{
    public class Drone : UnitBase, IMoveable, ICanAnimate
    {
#pragma warning disable 649
        [SerializeField] private DroneAIAgent ai;
        
        // TODO move inventory to inventory manager
        [SerializeField] private MainInventory inventory;
        [SerializeField] private SensorInventory sensorSlots;
        [SerializeField] private ActionInventory actionSlots;
        [SerializeField] private GoalInventory goalSlots;
#pragma warning restore 649

        private IMovement movement;
        private Animator animator;

        private void Awake()
        {
            ai.Init(gameObject);
            sensorSlots.Init(ai);
            actionSlots.Init(ai);
            goalSlots.Init(ai);
            
            movement = new BasicMovement(GetComponent<Rigidbody>());
            animator = GetComponent<Animator>();
            
            // TODO set these through abilities
            ai.Memory.Set(AIAttributes.CanMove, true);
            ai.Memory.Set(AIAttributes.CanPickupItems, true);
        }

        public void StartAI()
        {
            ai.Run();
        }

        public IMovement GetMovement() => movement;
        public Animator GetAnimator() => animator;

        public void PowerOn()
        {
            ai.Run();
        }

        public void PowerOff()
        {
            ai.Stop();   
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

            if (Input.GetKeyDown(KeyCode.I))
            {
                var inv = GameManager.UI.Get<InventoryUI>();
                if (inv.IsOpen)
                {
                    inv.Close();
                }
                else
                {
                    inv.Open();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var modulePrefab = Resources.Load<SensorModule>("DroneModules/Sensors/ResourceSensor");
                var module = Instantiate(modulePrefab);
                inventory.AddItem(module);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                var modulePrefab = Resources.Load<ActionModule>("DroneModules/Actions/PickupItem");
                var module = Instantiate(modulePrefab);
                inventory.AddItem(module);

                modulePrefab = Resources.Load<ActionModule>("DroneModules/Actions/FindResource");
                module = Instantiate(modulePrefab);
                inventory.AddItem(module);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                var modulePrefab = Resources.Load<GoalModule>("DroneModules/Goals/GatherIronGoal");
                var module = Instantiate(modulePrefab);
                inventory.AddItem(module);
                
                modulePrefab = Resources.Load<GoalModule>("DroneModules/Goals/PickupResource");
                module = Instantiate(modulePrefab);
                inventory.AddItem(module);
            }
        }
    }
}

