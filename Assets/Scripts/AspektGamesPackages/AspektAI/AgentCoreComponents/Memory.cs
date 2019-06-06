using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI
{
    // Memory holds everything the agent currently knows about the world and itself.
    // It is typically modified by a sensor, performing an action, or being acted upon.
    // Memory is the storage of state and does not perform any actions or observations.
    public class Memory<L, V> : IMemory<L, V>
    {
        private IAIAgent<L, V> agent;
        
        private readonly Dictionary<L, V> state = new Dictionary<L, V>();
        
        public void Init(IAIAgent<L, V> agent)
        {
            this.agent = agent;
        }

        public void Reset()
        {
            state.Clear();
        }

        public bool IsMatch(L label, V value)
        {
            if (state.ContainsKey(label))
            {
                return state[label].Equals(value);
            }

            // Agents are conservative and a "don't know" results in a failed condition check.
            // It's a prerequisite that a memory state is set in order to pass a condition.
            return false;
        }

        public void Set(L label, V value)
        {
            if (state.ContainsKey(label))
            {
                state[label] = value;
            }
            else
            {
                state.Add(label, value);
            }
        }

        public void Remove(L label)
        {
            state.Remove(label);
        }

        public Dictionary<L, V> GetState()
        {
            return state;
        }

        public Dictionary<L, V> CloneState()
        {
            return new Dictionary<L, V>(state);
        }
    }
}