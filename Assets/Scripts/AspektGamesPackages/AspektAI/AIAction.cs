using System.Collections.Generic;
using Aspekt.AI.Core;

namespace Aspekt.AI
{
    public abstract class AIAction<T, R> : IAIAction<T, R>
    {
        protected IAIAgent<T, R> agent;
        protected IMemory<T, R> memory;
        
        private enum States
        {
            NotInitialised, Enabled, Disabled
        }
        private States state = States.NotInitialised;
        
        public void Init(IAIAgent<T, R> agent, IMemory<T, R> memory)
        {
            this.agent = agent;
            this.memory = memory;

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

        public abstract bool Begin();

        public Dictionary<T, R> GetPrerequisites()
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<T, R> GetOutcomes()
        {
            throw new System.NotImplementedException();
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