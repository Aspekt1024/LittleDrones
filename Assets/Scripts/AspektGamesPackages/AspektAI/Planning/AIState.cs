using System.Collections.Generic;

namespace Aspekt.AI.Planning
{
    public class AIState<L, V>
    {
        private readonly Dictionary<L, V> state = new Dictionary<L, V>();
        private readonly Dictionary<L, V> effects = new Dictionary<L, V>();
        
        private readonly Dictionary<L, V> preconditions;

        public AIState()
        {
            preconditions = new Dictionary<L, V>();
        }

        public AIState(IAIGoal<L, V> goal, Dictionary<L, V> newState)
        {
            preconditions = new Dictionary<L, V>(goal.GetConditions());
            state = newState;
        }

        private AIState(AIState<L, V> oldState)
        {
            state = oldState.GetState();
            preconditions = new Dictionary<L, V>(oldState.GetPreconditions());
            effects = oldState.GetEffects();
        }

        public void ClearMetPreconditions(Dictionary<L, V> effects)
        {
            foreach (var effect in effects)
            {
                if (preconditions.ContainsKey(effect.Key) && preconditions[effect.Key].Equals(effect.Value))
                {
                    preconditions.Remove(effect.Key);
                }
            }
        }

        public void AddUnmetPreconditions(Dictionary<L, V> preconditionSet)
        {
            // Add preconditions not met by the world state
            foreach (var precondition in preconditionSet)
            {
                if (state.ContainsKey(precondition.Key) && state[precondition.Key].Equals(precondition.Value)) continue;
                AddPrecondition(precondition);
            }
        }

        public void AddPrecondition(KeyValuePair<L, V> precondition)
        {
            AddPrecondition(precondition.Key, precondition.Value);
        }

        public void AddPrecondition(L label, V value)
        {
            if (!preconditions.ContainsKey(label))
            {
                preconditions.Add(label, value);
            }
        }

        public Dictionary<L, V> GetState()
        {
            return state;
        }

        public Dictionary<L, V> GetPreconditions()
        {
            return preconditions;
        }

        public Dictionary<L, V> GetEffects()
        {
            return effects;
        }

        public AIState<L, V> Clone()
        {
            return new AIState<L, V>(this);
        }
    }
}