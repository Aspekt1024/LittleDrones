using Aspekt.AI.Core;
using UnityEngine;

namespace Aspekt.AI
{
    public abstract class Sensor : ISensor
    {
        protected IAIAgent agent;
        protected IMemory memory;
        
        private enum States
        {
            NotInitialised, Enabled, Disabled
        }
        private States state = States.NotInitialised;

        public void Init(IAIAgent agent, IMemory memory)
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
                Sense(deltaTime);
            }
        }

        public void Enable()
        {
            state = States.Enabled;
        }

        public void Disable()
        {
            state = States.Disabled;
        }
        
        public void Remove()
        {
            OnRemove();
        }

        /// <summary>
        /// Called after Init to allow child classes to have custom setup options
        /// </summary>
        protected virtual void OnInit() {}
        
        /// <summary>
        /// Called once per frame
        /// </summary>
        protected abstract void Sense(float deltaTime);
        
        /// <summary>
        /// Called when the sensor is removed to allow cleanup (e.g. removing memory objects)
        /// </summary>
        protected virtual void OnRemove() {}
    }
}