using Pathfinding;
using UnityEngine;

namespace Aspekt.Drones
{
    public class Drone : Unit
    {
#pragma warning disable 649
        [SerializeField] private DroneVitals vitals;
        [SerializeField] private DroneAIAgent ai;
        
        // TODO move inventory to inventory manager
        [SerializeField] private MainInventory inventory;
        [SerializeField] private SensorInventory sensorSlots;
        [SerializeField] private ActionInventory actionSlots;
        [SerializeField] private GoalInventory goalSlots;
#pragma warning restore 649
        
        private Animator animator;

        public override IAbilityManager Abilities { get; } = new AbilityManager();

        public DroneVitals Vitals => vitals;
        
        private void Awake()
        {
            InitialiseDroneAbilities();
            animator = GetComponent<Animator>();
            
            ai.Init(gameObject);
            sensorSlots.Init(ai);
            actionSlots.Init(ai);
            goalSlots.Init(ai);
            
            vitals.SetUsageRate(0.2f);
            vitals.IsConsumingFuel = false;
        }

        public void PowerOn()
        {
            Debug.Log("power on");
            vitals.IsConsumingFuel = true;
            ai.Run();
        }

        public void PowerOff()
        {
            Debug.Log("power off");
            vitals.IsConsumingFuel = false;
            ai.Stop();   
        }

        public void AddSensor(SensorModule sensor) => sensorSlots.AddItem(sensor);
        public void AddAction(ActionModule action) => actionSlots.AddItem(action);
        public void AddGoal(GoalModule goal) => goalSlots.AddItem(goal);
        
        private void Update()
        {
            if (vitals.CurrentFuel <= 0f)
            {
                Debug.Log("drone ran out of fuel and is dead forever because that's realistic");
                ai.Stop();
                Remove();
                return;
            }
            
            vitals.Tick(Time.deltaTime);
            
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
        }

        private void InitialiseDroneAbilities()
        {
            var movement = new GroundMovement(GetComponent<Rigidbody>(), GetComponent<RichAI>());
            var gatherer = new GatherComponent(this);
            var worker = new WorkerComponent(this);
            
            Abilities.AddAbility(movement);
            Abilities.AddAbility(gatherer);
            Abilities.AddAbility(worker);
        }
    }
}

