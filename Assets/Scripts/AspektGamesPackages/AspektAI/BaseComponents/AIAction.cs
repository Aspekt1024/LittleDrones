using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.AI.Internal;
using UnityEditor.UIElements;
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

        public virtual bool CheckProceduralPreconditions()
        {
            return state == States.Enabled;
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

        protected abstract void SetPreconditions();
        protected abstract void SetEffects();

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

        protected void ActionFailure() => OnActionFailed?.Invoke();
        protected void ActionSuccess() => OnActionSucceeded?.Invoke();
    }
}