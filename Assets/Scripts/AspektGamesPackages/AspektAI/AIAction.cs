using System;
using System.Collections.Generic;

namespace Aspekt.AI
{
    public abstract class AIAction<L, V> : IAIAction<L, V>
    {
        protected IAIAgent<L, V> agent;
        
        private enum States
        {
            NotInitialised, Enabled, Disabled
        }
        private States state = States.NotInitialised;

        public abstract float Cost { get; }

        public void Init(IAIAgent<L, V> agent)
        {
            this.agent = agent;
            state = States.Enabled;
            OnInit();
        }

        public void Tick(float deltaTime)
        {
            if (state == States.Enabled)
            {
                OnTick(deltaTime);
            }
        }
        
        public abstract bool Begin(IStateMachine<L, V> stateMachine, Action onSuccessCallback, Action onFailureCallback);

        public abstract Dictionary<L, V> GetPreconditions();

        public virtual bool CheckProceduralPreconditions()
        {
            return state == States.Enabled;
        }
        
        public abstract Dictionary<L, V> GetEffects();

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

        public abstract bool IsComplete();

        protected abstract void OnTick(float deltaTime);
        
        /// <summary>
        /// Called after Init to allow child classes to have custom setup options
        /// </summary>
        protected virtual void OnInit() {}

        /// <summary>
        /// Called when the action is removed to allow cleanup (e.g. exiting co-routines safely)
        /// </summary>
        protected abstract void OnRemove();

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
    }
}