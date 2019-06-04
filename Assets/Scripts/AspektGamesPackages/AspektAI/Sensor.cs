using Aspekt.AI.Core;
using UnityEngine;

namespace Aspekt.AI
{
    public abstract class Sensor<T, R> : ISensor<T, R>
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
        
        protected abstract void OnTick(float deltaTime);
        
        /// <summary>
        /// Called after Init to allow child classes to have custom setup options
        /// </summary>
        protected virtual void OnInit() {}

        /// <summary>
        /// Called when the sensor is removed to allow cleanup (e.g. removing memory objects)
        /// </summary>
        protected abstract void OnRemove();

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
    }
}