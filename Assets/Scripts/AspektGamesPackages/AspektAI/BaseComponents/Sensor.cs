
using System.Collections.Generic;

namespace Aspekt.AI
{
    public abstract class Sensor<L, V> : ISensor<L, V>
    {
        protected IAIAgent<L, V> Agent;
        
        private enum States
        {
            NotInitialised, Enabled, Disabled
        }
        private States state = States.NotInitialised;

        public bool IsEnabled => state == States.Enabled;

        public void Init(IAIAgent<L, V> agent)
        {
            Agent = agent;
            OnInit();
            state = States.Enabled;
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
        }

        public void Disable()
        {
            state = States.Disabled;
        }
        
        public void Remove()
        {
            state = States.Disabled;
        }
        
        protected abstract void OnTick(float deltaTime);
        
        /// <summary>
        /// Called after Init to allow child classes to have custom setup options
        /// </summary>
        protected virtual void OnInit() {}
    }
}