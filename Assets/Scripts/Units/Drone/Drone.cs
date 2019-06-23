using Pathfinding;
using UnityEngine;

namespace Aspekt.Drones
{
    public class Drone : UnitBase
    {
#pragma warning disable 649
        [SerializeField] private DroneAIAgent ai;
        
        // TODO move inventory to inventory manager
        [SerializeField] private MainInventory inventory;
        [SerializeField] private SensorInventory sensorSlots;
        [SerializeField] private ActionInventory actionSlots;
        [SerializeField] private GoalInventory goalSlots;
#pragma warning restore 649
        
        private Animator animator;

        public override IAbilityManager Abilities { get; } = new AbilityManager();
        
        private void Awake()
        {
            InitialiseDroneAbilities();
            animator = GetComponent<Animator>();
            
            ai.Init(gameObject);
            sensorSlots.Init(ai);
            actionSlots.Init(ai);
            goalSlots.Init(ai);
        }

        public void StartAI()
        {
            ai.Run();
        }
        
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
                sensorSlots.AddItem(module);
                
                modulePrefab = Resources.Load<SensorModule>("DroneModules/Sensors/BuildingSensor");
                module = Instantiate(modulePrefab);
                sensorSlots.AddItem(module);
                
                modulePrefab = Resources.Load<SensorModule>("DroneModules/Sensors/ObjectSensor");
                module = Instantiate(modulePrefab);
                sensorSlots.AddItem(module);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                var modulePrefab = Resources.Load<ActionModule>("DroneModules/Actions/PickupItem");
                var module = Instantiate(modulePrefab);
                actionSlots.AddItem(module);

                modulePrefab = Resources.Load<ActionModule>("DroneModules/Actions/FindResource");
                module = Instantiate(modulePrefab);
                actionSlots.AddItem(module);
                
                modulePrefab = Resources.Load<ActionModule>("DroneModules/Actions/StoreItem");
                module = Instantiate(modulePrefab);
                actionSlots.AddItem(module);
                
                modulePrefab = Resources.Load<ActionModule>("DroneModules/Actions/GatherFromDeposit");
                module = Instantiate(modulePrefab);
                actionSlots.AddItem(module);
                
                modulePrefab = Resources.Load<ActionModule>("DroneModules/Actions/FindDeposit");
                module = Instantiate(modulePrefab);
                actionSlots.AddItem(module);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                var modulePrefab = Resources.Load<GoalModule>("DroneModules/Goals/GatherIronGoal");
                var module = Instantiate(modulePrefab);
                goalSlots.AddItem(module);
                
//                modulePrefab = Resources.Load<GoalModule>("DroneModules/Goals/PickupResource");
//                module = Instantiate(modulePrefab);
//                goalSlots.AddItem(module);
            }
        }

        private void InitialiseDroneAbilities()
        {
            var movement = new GroundMovement(GetComponent<Rigidbody>(), GetComponentInChildren<Seeker>());
            var gatherer = new GatherComponent();
            Abilities.AddAbility(movement);
            Abilities.AddAbility(gatherer);
        }
    }
}

