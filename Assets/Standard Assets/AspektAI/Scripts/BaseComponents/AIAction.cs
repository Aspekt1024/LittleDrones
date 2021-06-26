using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.AI
{
    public abstract class AIAction<L, V> : IAIAction<L, V>
    {
        protected IAIAgent<L, V> Agent;
        public event Action OnActionSucceeded;
        public event Action OnActionFailed;
        
        private enum States
        {
            NotInitialised, Enabled, Disabled
        }
        private States state = States.NotInitialised;

        private readonly Dictionary<L, V> preconditions = new Dictionary<L, V>();
        private readonly Dictionary<L, V> effects = new Dictionary<L, V>();

        public abstract float Cost { get; }

        public void Init(IAIAgent<L, V> agent)
        {
            Agent = agent;
            state = States.Enabled;
            SetPreconditions();
            SetEffects();
            OnInit();
        }

        public void Tick(float deltaTime)
        {
            if (state == States.Enabled)
            {
                OnTick(deltaTime);
            }
        }

        public bool Enter(IStateMachine<L, V> stateMachine)
        {
            return Begin(stateMachine);
        }

        public void Stop()
        {
            OnStop();
        }

        public Dictionary<L, V> GetPreconditions() => preconditions;

        public abstract bool CheckComponents();

        public Dictionary<L, V> GetEffects()
        {
            if (!effects.Any())
            {
                Debug.LogError($"{GetType().Name} has no effects.");
            }
            return effects;
        }

        public bool CheckProceduralPreconditions()
        {
            return state == States.Enabled && CheckProceduralConditions();
        }

        public void Enable()
        {
            state = States.Enabled;
            OnEnable();
        }

        public void Disable()
        {
            state = States.Disabled;
            OnDisable();
        }

        public virtual void OnRemove()
        {
            OnActionFailed?.Invoke();
            state = States.Disabled;
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        
        /// <summary>
        /// Add preconditions (using <see cref="AddPrecondition"/>) that must be true for this action to be used.
        /// Checked during planning.
        /// </summary>
        protected abstract void SetPreconditions();
        
        /// <summary>
        /// Add effects (using <see cref="AddEffect"/>) that are applied when the action is complete.
        /// </summary>
        protected abstract void SetEffects();
        
        /// <summary>
        /// Called each frame. Returning false at any point results in a failed action.
        /// This is not checked during the planning stage.
        /// </summary>
        protected abstract bool CheckProceduralConditions();

        protected void AddPrecondition(L label, V value)
        {
            if (preconditions.ContainsKey(label))
            {
                preconditions[label] = value;
            }
            else
            {
                preconditions.Add(label, value);
            }
        }

        protected void AddEffect(L label, V value)
        {
            if (effects.ContainsKey(label))
            {
                effects[label] = value;
            }
            else
            {
                effects.Add(label, value);
            }
        }
        
        protected abstract void OnTick(float deltaTime);
        
        /// <summary>
        /// Called after Init to allow child classes to have custom setup options
        /// </summary>
        protected virtual void OnInit() {}
        
        /// <summary>
        /// Called when the action begins operation
        /// </summary>
        /// <param name="stateMachine">The AI agent's state machine</param>
        /// <returns>True if the action successfully began</returns>
        protected abstract bool Begin(IStateMachine<L, V> stateMachine);

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnStop() { }

        protected void ActionFailure() => OnActionFailed?.Invoke();
        protected virtual void ActionSuccess() => OnActionSucceeded?.Invoke();
    }
}