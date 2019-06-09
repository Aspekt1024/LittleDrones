using System;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace Aspekt.AI
{
    public abstract class AIAction<L, V> : IAIAction<L, V>
    {
        protected IAIAgent<L, V> Agent;
        protected Action ActionSuccess;
        protected Action ActionFailure;
        
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

        public bool Enter(IStateMachine<L, V> stateMachine, Action onSuccessCallback, Action onFailureCallback)
        {
            ActionSuccess = onSuccessCallback;
            ActionFailure = onFailureCallback;
            return Begin(stateMachine);
        }

        public Dictionary<L, V> GetPreconditions() => preconditions;
        public Dictionary<L, V> GetEffects() => effects;

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

        public void Remove()
        {
            OnRemove();
        }

        protected abstract void SetPreconditions();
        protected abstract void SetEffects();

        protected void AddPrecondition(L label, V value)
        {
            preconditions.Add(label, value);
        }

        protected void AddEffect(L label, V value)
        {
            effects.Add(label, value);
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
        
        /// <summary>
        /// Called when the action is removed to allow cleanup (e.g. exiting co-routines safely)
        /// </summary>
        protected abstract void OnRemove();

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
    }
}