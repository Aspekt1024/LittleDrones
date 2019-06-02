namespace Aspekt.AI.Core
{
    // Manages the operation of the agent's actions
    public class StateMachine : IStateMachine
    {
        private IAIAgent agent;
        
        private enum States
        {
            Paused, Stopped, Running
        }
        private States state = States.Stopped;

        public void Init(IAIAgent agent)
        {
            this.agent = agent;
        }
        
        public void Start()
        {
            if (state == States.Running) return;
            state = States.Running;
            // TODO begin operation
        }

        public void Stop()
        {
            if (state == States.Stopped) return;
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