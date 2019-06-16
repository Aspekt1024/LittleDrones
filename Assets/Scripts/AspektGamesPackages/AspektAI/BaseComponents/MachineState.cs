using System;

namespace Aspekt.AI
{
    public abstract class MachineState<L, V> : IMachineState<L, V>
    {
        protected readonly IAIAgent<L, V> Agent;
        
        public event Action OnComplete = delegate { };

        public MachineState(IAIAgent<L, V> agent)
        {
            Agent = agent;
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public abstract void Start();
        public abstract void Pause();
        public abstract void Stop();

        public override string ToString()
        {
            return GetType().Name;
        }

        protected void StateComplete()
        {
            OnComplete?.Invoke();
        }
    }
}