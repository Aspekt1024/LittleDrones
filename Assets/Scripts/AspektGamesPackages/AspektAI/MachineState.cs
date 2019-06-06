using System;

namespace Aspekt.AI
{
    public abstract class MachineState<L, V> : IMachineState<L, V>
    {
        protected IAIAgent<L, V> agent;
        
        public event Action OnComplete = delegate { };
        
        public void Init(IAIAgent<L, V> parentAgent)
        {
            agent = parentAgent;
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public abstract void Start();
        public abstract void Pause();
        public abstract void Stop();

        public override string ToString()
        {
            return GetType().ToString();
        }

        protected void StateComplete()
        {
            OnComplete?.Invoke();
        }
    }
}