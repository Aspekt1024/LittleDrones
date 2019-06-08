namespace Aspekt.AI.Planning
{
    /// <summary>
    /// A node in the AI agent's action plan. Nodes come in one of two varieties: action nodes and goal nodes
    /// </summary>
    public class AINode<L, V>
    {
        private float g;    // Node Cost
        private float h;    // Heuristic

        private IAIAgent<L, V> agent;
        private IAIAction<L, V> action;
        private IAIGoal<L, V> goal;
        private AINode<L, V> parent;
        private AIState<L, V> state;

        public AINode(IAIAgent<L, V> agent, IAIGoal<L, V> goal, IAIAction<L, V> action = null, AINode<L, V> parent = null)
        {
            this.agent = agent;
            this.action = action;
            this.parent = parent;

            if (action == null)
            {
                // This is a goal node
                state = new AIState<L, V>(goal, agent.Memory.CloneState());
                state.AddUnmetPreconditions(goal.GetConditions());
                g = 0;
            }
            else
            {
                // This is an action node
                state = new AIState<L, V>();
                g = float.MaxValue;
            }
        }

        public void Update(AINode<L, V> newParent)
        {
            if (newParent.g + action.Cost < g)
            {
                parent = newParent;
                SetNodeActionDetails();
            }
        }

        private void SetNodeActionDetails()
        {
            g = parent.g + action.Cost;
            state = parent.state.Clone();
            state.ClearMetPreconditions(action.GetEffects());
            state.AddUnmetPreconditions(action.GetPreconditions());
            h = GetNumUnmetPreconditions();
        }

        private int GetNumUnmetPreconditions()
        {
            return state.GetPreconditions().Count;
        }

        public AIState<L, V> GetState() => state;
        public IAIAction<L, V> GetAction() => action;
        public AINode<L, V> GetParent() => parent;

        public bool ConditionsMet()
        {
            if (state.GetPreconditions().Count == 0) return true;
            
            foreach (var precondition in state.GetPreconditions())
            {
                if (!agent.Memory.IsMatch(precondition)) return false;
            }

            return true;
        }

        public float GetFCost()
        {
            return g + h;
        }
    }
}