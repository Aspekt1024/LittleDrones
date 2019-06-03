namespace Aspekt.AI.Core
{
    // Manages the operation of the agent's actions
    public class StateMachine<T, R> : IStateMachine<T, R>
    {
        private IAIAgent<T, R> agent;
        
        private enum States
        {
            Paused, Stopped, Running
        }
        private States state = States.Stopped;

        public void Init(IAIAgent<T, R> agent)
        {
            this.agent = agent;
        }
        
        public void Start()
        {
            state = States.Running;
            // TODO begin operation
        }

        public void Stop()
        {
            state = States.Stopped;
        }

        public void Pause()
        {
            if (state == States.Paused || state == States.Stopped) return;
            state = States.Paused;
            
        }

        public void Tick(float deltaTime)
        {
            if (state == States.Paused || state == States.Stopped) return;
        }
    }
}