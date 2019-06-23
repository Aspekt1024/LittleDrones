using Aspekt.AI.Internal;
using UnityEngine;

namespace Aspekt.AI
{
    // Attached to a game object to give it the power of thought
    public abstract class AIAgent<L, V> : MonoBehaviour, IAIAgent<L, V>
    {
        public AILogger.LogLevels logLevel;
        public enum UpdateModes
        {
            OnDemand, Periodic
        }
        public UpdateModes updateMode = UpdateModes.OnDemand;
        public float updateInterval = 1f;
        
        public GameObject Owner { get; private set; }
        public Transform Transform => transform;
        public IMemory<L, V> Memory { get; } = new Memory<L, V>();
        public IActionController<L, V> Actions { get; } = new ActionController<L, V>();
        public ISensorController<L, V> Sensors { get; } = new SensorController<L, V>();
        public IGoalController<L, V> Goals { get; } = new GoalController<L, V>();
        public AILogger Logger { get; private set; }

        private IExecutor<L, V> executor;
        private IPlanner<L, V> planner;
        
        private enum States
        {
            NotInitialised, AwaitingActionPlan, Running, Stopped, Paused,
            Idle
        }
        private States state = States.NotInitialised;

        private bool calculateGoalRequested;
        private float timeLastUpdated;

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
            
            Logger = new AILogger(logLevel);

            planner.OnActionPlanFound += OnActionPlanFound;
            planner.OnActionPlanNotFound += OnActionPlanNotFound;
            executor.OnActionPlanComplete += OnActionPlanComplete;

            OnInit();
        }

        /// <summary>
        /// Called after Init, this can be overridden to add additional setup for the agent
        /// </summary>
        protected virtual void OnInit() { }
        
        private void Start()
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
                executor.Stop();
                state = States.AwaitingActionPlan;
                calculateGoalRequested = false;
                planner.CalculateNewGoal();
            }

            if (CanUpdatePeriodically)
            {
                QueueGoalCalculation();
                return;
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

            executor.Resume();
            Sensors.Enable();
            state = States.Running;
        }

        public void Pause()
        {
            Sensors.Disable();
            executor.Pause();
            state = States.Paused;
        }

        public void Stop()
        {
            Sensors.Disable();
            Memory.Reset();
            executor.Stop();
            state = States.Stopped;
        }

        public void LogTrace<T>(T parent, string message) => Logger.Log(AILogType.Trace, parent, message);
        public void LogInfo<T>(T parent, string message) => Logger.Log(AILogType.Info, parent, message);
        public void LogKeyInfo<T>(T parent, string message) => Logger.Log(AILogType.KeyInfo, parent, message);

        protected virtual void OnActionPlanComplete() { }
        
        protected string GetMemoryStatus()
        {
            var status = "<b>Memory:</b>";
            foreach (var s in Memory.GetState())
            {
                status += $"\n{s.Key} : {s.Value}";
            }
            return status;
        }

        protected string GetExecutorStatus()
        {
            var status = "<b>Executor:</b> ";
            status += executor.GetStatus();
            return status;
        }

        private void OnActionPlanFound()
        {
            timeLastUpdated = Time.time;
            executor.ExecutePlan(planner.GetActionPlan(), planner.GetGoal());
            if (state == States.AwaitingActionPlan)
            {
                state = States.Running;
            }
        }

        private bool CanUpdatePeriodically => state == States.Idle &&
                                              updateMode == UpdateModes.Periodic &&
                                              !calculateGoalRequested &&
                                              Time.time > timeLastUpdated + updateInterval;

        private void OnActionPlanNotFound()
        {
            timeLastUpdated = Time.time;
            state = States.Idle;
        }

        private void OnDestroy()
        {
            if (planner != null)
            {
                planner.OnActionPlanFound -= OnActionPlanFound;
                planner.OnActionPlanNotFound -= OnActionPlanNotFound;
            }

            if (executor != null)
            {
                executor.OnActionPlanComplete -= OnActionPlanComplete;
            }
        }
    }
}