using System;
using System.Collections.Generic;
using Aspekt.AI.Internal;
using Aspekt.AI.Planning;
using UnityEngine;

namespace Aspekt.AI
{
    /// <summary>
    /// Attached to a game object to give it the power of thought
    /// </summary>
    public abstract class AIAgent<L, V> : MonoBehaviour, IAIAgent<L, V>
    {
        public AILogger.LogLevels logLevel;
        public enum UpdateModes
        {
            OnDemand, Periodic
        }
        public UpdateModes updateMode = UpdateModes.OnDemand;
        public float updateInterval = 1f;
        
        private const float QueueTime = 0.1f;
        private float timeQueued;
        
        public GameObject Owner { get; private set; }
        public Transform Transform => transform;
        public IMemory<L, V> Memory { get; } = new Memory<L, V>();
        public IActionController<L, V> Actions { get; } = new ActionController<L, V>();
        public ISensorController<L, V> Sensors { get; } = new SensorController<L, V>();
        public IGoalController<L, V> Goals { get; } = new GoalController<L, V>();
        public AILogger Logger { get; private set; }
        
        public bool IsExecuting => executor.IsExecuting;
        public bool IsIdle => state == States.Idle;

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

        public interface IObserver
        {
            void OnAgentPlanCalculated(Queue<IAIAction<L, V>> newActionPlan, IAIGoal<L, V> goal);
            void OnAgentPlanCalculationFailure();
        }

        private readonly List<IObserver> observers = new List<IObserver>();
        public void RegisterObserver(IObserver observer) => observers.Add(observer);
        public void UnregisterObserver(IObserver observer) => observers.Remove(observer);

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
        
        /// <summary>
        /// Called after Run. Override to add additional functions before execution
        /// </summary>
        protected virtual void OnRun() { }
        
        /// <summary>
        /// Called when the AI Agent is stopped. Override to add additional functions when interrupted by a Stop command
        /// </summary>
        protected virtual void OnStop() { }

        private void Update()
        {
            //Only update AI if the game has initialised fully
            if (state == States.NotInitialised) return;

            Tick();
            
            if (calculateGoalRequested && Time.time > timeQueued + QueueTime)
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

            if (state == States.AwaitingActionPlan || state == States.Running)
            {
                Sensors.Tick(Time.deltaTime);
            }
            
            if (state == States.Running)
            {
                executor.Tick(Time.deltaTime);
            }
        }
        
        public void Run()
        {
            if (state == States.NotInitialised)
            {
                Debug.LogError("Agent.Init() has not been called. This agent will not function.");
            }
            
            state = States.Running;
            Sensors.Enable();
            OnRun();
            QueueGoalCalculation();
        }

        public void QueueGoalCalculation()
        {
            timeQueued = Time.time;
            state = States.AwaitingActionPlan;
            calculateGoalRequested = true;
        }

        public void CalculateGoalDryRun(Action<ActionPlan<L, V>> callback)
        {
            planner.CalculateNewGoalDryRun(callback);
        }

        public void RunActionPlan(ActionPlan<L, V> actionPlan)
        {
            executor.ExecutePlan(actionPlan.Actions, actionPlan.Goal);
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
            executor.Stop();
            state = States.Stopped;
            OnStop();
        }

        public void Shutdown()
        {
            Memory.Reset();
            Stop();
        }

        public void LogTrace<T>(T parent, string message) => Logger.Log(AILogType.Trace, parent, message);
        public void LogInfo<T>(T parent, string message) => Logger.Log(AILogType.Info, parent, message);
        public void LogKeyInfo<T>(T parent, string message) => Logger.Log(AILogType.KeyInfo, parent, message);

        protected virtual void OnActionPlanComplete(bool success) { }
        protected virtual void Tick() { }
        
        public string GetMemoryStatus()
        {
            if (Memory == null) return "Memory has been destroyed";
            
            var status = "<b>Memory:</b>";
            foreach (var s in Memory.GetState())
            {
                status += $"\n{s.Key} : {s.Value}";
            }
            return status;
        }

        public string GetExecutorStatus()
        {
            if (executor == null) return "Executor has been destroyed";
            
            var status = "<b>Executor:</b> ";
            status += executor.GetStatus();
            return status;
        }

        public void SetDiagnosticsStatus(bool isActive) => planner.SetDiagnosticsStatus(isActive);
        public PlannerDiagnosticData<L, V> GetDiagnostics() => planner.GetDiagnostics();

        private void OnActionPlanFound()
        {
            observers.ForEach(o => o.OnAgentPlanCalculated(planner.GetActionPlan(), planner.GetGoal()));
            
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
            observers.ForEach(o => o.OnAgentPlanCalculationFailure());
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