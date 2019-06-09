
namespace Aspekt.AI
{
    public abstract class Sensor<L, V> : ISensor<L, V>
    {
        protected IAIAgent<L, V> agent;
        
        private enum States
        {
            NotInitialised, Enabled, Disabled
        }
        private States state = States.NotInitialised;

        public abstract L[] Effects { get; } 
        
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