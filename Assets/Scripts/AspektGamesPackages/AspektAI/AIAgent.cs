using UnityEngine;

namespace Aspekt.AI
{
    // Attached to a game object to give it the power of thought
    public abstract class AIAgent<L, V> : MonoBehaviour, IAIAgent<L, V>
    {
        public GameObject Owner { get; private set; }
        public Transform Transform => transform;
        public IMemory<L, V> Memory { get; } = new Memory<L, V>();
        public IActionController<L, V> Actions { get; } = new ActionController<L, V>();
        public ISensorController<L, V> Sensors { get; } = new SensorController<L, V>();
        public IGoalController<L, V> Goals { get; } = new GoalController<L, V>();
        
        private readonly AILogger logger = new AILogger(AILogger.LogLevels.Debug);

        private enum States
        {
            NotInitialised, AwaitingActionPlan, Running, Stopped, Paused
        }

        private States state = States.NotInitialised;

        private bool calculateGoalRequested;

        protected IExecutor<L, V> executor;
        private IPlanner<L, V> planner;

        public void Init(GameObject owner)
        {
            state = States.Stopped;
            
            Owner = owner;
            Memory.Init(this);
            Actions.Init(this);
            Sensors.Init(this);
            Goals.Init(this);
            
            executor = new Executor<L, V>(this);
            planner = new Planner<L, V>(this);

            planner.OnActionPlanFound += OnActionPlanFound;
            executor.OnActionPlanComplete += OnActionPlanComplete;
        }
        
        protected virtual void Start()
        {
            if (state == States.NotInitialised)
            {
                Debug.LogError("Agent.Init() has not been called. This agent will not function.");
            }
        }

        private void Update()
        {
            if (calculateGoalRequested)
            {
                state = States.AwaitingActionPlan;
                calculateGoalRequested = false;
                planner.CalculateNewGoal();
            }
            
            if (state != States.Running) return;
            
            Sensors.Tick(Time.deltaTime);
            executor.Tick(Time.deltaTime);
        }
        
        public void Run()
        {
            if (state == States.NotInitialised)
            {
                Debug.LogError("Agent.Init() has not been called. This agent will not function.");
            }
            
            state = States.Running;
            OnRun();
            QueueGoalCalculation();
        }

        public void QueueGoalCalculation()
        {
            state = States.AwaitingActionPlan;
            calculateGoalRequested = true;
        }

        public void Resume()
        {
            if (state != States.Paused)
            {
                Debug.LogWarning("Attempted to resume Agent from non-paused state. This has no effect");
                return;
            }

            state = States.Running;
        }

        public void Pause()
        {
            Sensors.DisableSensors();
        }

        public void Stop()
        {
            Sensors.DisableSensors();
            Memory.Reset();
        }

        public void LogTrace<T>(T parent, string message) => logger.Log(AILogType.Trace, parent, message);
        public void LogInfo<T>(T parent, string message) => logger.Log(AILogType.Info, parent, message);
        public void LogKeyInfo<T>(T parent, string message) => logger.Log(AILogType.KeyInfo, parent, message);

        protected virtual void OnActionPlanComplete() { }

        /// <summary>
        /// Called when Run() is called, and before the planner calculates an action plan
        /// </summary>
        protected virtual void OnRun() { }

        private void OnActionPlanFound()
        {
            executor.ExecutePlan(planner.GetActionPlan(), planner.GetGoal());
            if (state == States.AwaitingActionPlan)
            {
                state = States.Running;
            }
        }
    }
}