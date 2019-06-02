using System.Collections.Generic;

namespace Aspekt.AI.Core
{
    // Memory holds everything the agent currently knows about the world and itself.
    // It is typically modified by a sensor, performing an action, or being acted upon.
    // Memory is the storage of state and does not perform any actions or observations.
    public class Memory : IMemory
    {
        private IAIAgent agent;
        
        private readonly Dictionary<object, object> memory = new Dictionary<object, object>();
        
        public void Init(IAIAgent agent)
        {
            this.agent = agent;
        }

        public void Reset()
        {
            memory.Clear();
        }

        public object Get(object key)
        {
            if (memory.ContainsKey(key))
            {
                return memory[key];
            }
            return null;
        }

        public V Get<K, V>(K key)
        {
            if (memory.ContainsKey(key))
            {
                return (V)memory[key];
            }
            return default;
        }

        /// <summary>
        /// Retrieves the memory value and returns true if the memory key exists.
        /// If the memory key doesn't exist, this returns false and the default value is given.
        /// </summary>
        /// <param name="key">The memory key</param>
        /// <param name="value">The memory value</param>
        /// <typeparam name="K">The memory key type</typeparam>
        /// <typeparam name="V">The memory value type</typeparam>
        /// <returns>success</returns>
        public bool TryGet<K, V>(K key, out V value)
        {
            if (memory.ContainsKey(key))
            {
                value = (V)memory[key];
                return true;
            }

            value = default;
            return false;
        }

        public void Set(object key, object value)
        {
            if (memory.ContainsKey(key))
            {
                memory[key] = value;
            }
            else
            {
                memory.Add(key, value);
            }
        }

        public void Remove(object key)
        {
            memory.Remove(key);
        }
    }
}