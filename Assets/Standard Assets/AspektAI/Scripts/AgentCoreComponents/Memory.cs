using System;
using System.Collections.Generic;

namespace Aspekt.AI.Internal
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
                var comparer = Comparer<V>.Default;
                return comparer.Compare(state[label], value) == 0;
            }

            // Agents are conservative and a "don't know" results in a failed condition check.
            // It's a prerequisite that a memory state is set in order to pass a condition.
            return false;
        }

        public bool IsTrue(L label) => state.ContainsKey(label) && state[label].Equals(true);
        public bool IsFalse(L label) => state.ContainsKey(label) && state[label].Equals(false);

        public bool IsMatch(KeyValuePair<L, V> pair)
        {
            return IsMatch(pair.Key, pair.Value);
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

        public T Get<T>(L label) where T : V
        {
            var v = Get(label);
            if (EqualityComparer<V>.Default.Equals(v, default)) return default;
            try
            {
                return (T) v;
            }
            catch
            {
                return default;
            }
        }

        public V Get(L label)
        {
            if (state.ContainsKey(label))
            {
                return state[label];
            }
            return default;
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